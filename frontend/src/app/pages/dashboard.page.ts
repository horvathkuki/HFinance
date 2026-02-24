import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { BaseChartDirective } from 'ng2-charts';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { AnalyticsService } from '../analytics.service';
import { GroupsService } from '../groups.service';
import { PortfolioService } from '../portfolio.service';
import { Holding, HoldingGroup, Portfolio, PortfolioAnalytics, Snapshot, TimeSeriesPoint } from '../models';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, BaseChartDirective],
  template: `
    <section class="page">
      <h1>Portfolio Dashboard</h1>

      <div class="cards">
        <article>
          <h2>Portfolios</h2>
          <form [formGroup]="portfolioForm" (ngSubmit)="createPortfolio()">
            <input type="text" placeholder="Portfolio name" formControlName="name" />
            <input type="text" placeholder="Description" formControlName="description" />
            <button type="submit">Create</button>
          </form>
          <ul>
            <li *ngFor="let portfolio of portfolios()">
              <button class="select" (click)="selectPortfolio(portfolio)">
                {{ portfolio.name }}
              </button>
              <button class="delete" (click)="deletePortfolio(portfolio.id)">Delete</button>
            </li>
          </ul>
        </article>

        <article>
          <h2>Groups</h2>
          <form [formGroup]="groupForm" (ngSubmit)="createGroup()">
            <input type="text" placeholder="Group name" formControlName="name" />
            <button type="submit">Add Group</button>
          </form>
          <ul>
            <li *ngFor="let group of groups()">
              {{ group.name }}
              <button class="delete" (click)="deleteGroup(group.id)">Delete</button>
            </li>
          </ul>
        </article>
      </div>

      <article *ngIf="selectedPortfolio() as currentPortfolio">
        <h2>{{ currentPortfolio.name }} - Holdings</h2>
        <form [formGroup]="holdingForm" (ngSubmit)="addHolding()">
          <input type="text" placeholder="Symbol" formControlName="symbol" />
          <input type="number" step="0.0001" placeholder="Quantity" formControlName="quantity" />
          <input
            type="number"
            step="0.01"
            placeholder="Average purchase price"
            formControlName="averagePurchasePrice"
          />
          <select formControlName="currency">
            <option value="EUR">EUR</option>
            <option value="USD">USD</option>
            <option value="RON">RON</option>
          </select>
          <select formControlName="groupId">
            <option *ngFor="let group of groups()" [value]="group.id">{{ group.name }}</option>
          </select>
          <button type="submit">Add holding</button>
        </form>

        <table *ngIf="holdings().length > 0">
          <thead>
            <tr>
              <th>Symbol</th>
              <th>Qty</th>
              <th>Avg Price</th>
              <th>Currency</th>
              <th>Group</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let holding of holdings()">
              <td>{{ holding.symbol }}</td>
              <td>{{ holding.quantity }}</td>
              <td>{{ holding.averagePurchasePrice }}</td>
              <td>{{ holding.currency }}</td>
              <td>{{ holding.groupName }}</td>
              <td><button class="delete" (click)="deleteHolding(holding)">Delete</button></td>
            </tr>
          </tbody>
        </table>

        <div class="analytics" *ngIf="analytics() as metrics">
          <p><strong>Base Currency:</strong> {{ metrics.baseCurrency }}</p>
          <p><strong>Total Market Value:</strong> {{ metrics.totalMarketValueBase | number:'1.2-2' }}</p>
          <p><strong>Total P/L:</strong> {{ metrics.totalUnrealizedPnLBase | number:'1.2-2' }}</p>
          <p *ngIf="metrics.isPartial">Snapshot/analytics are partial. Missing symbols: {{ metrics.missingSymbolsCount }}</p>
          <button (click)="takeSnapshot()">Take Snapshot</button>
        </div>

        <div class="charts">
          <div>
            <h3>Portfolio Value Over Time</h3>
            <canvas baseChart [data]="valueChartData" [type]="'line'" [options]="lineOptions"></canvas>
          </div>
          <div>
            <h3>Allocation by Group</h3>
            <canvas baseChart [data]="allocationChartData" [type]="'doughnut'"></canvas>
          </div>
          <div>
            <h3>Unrealized P/L Trend</h3>
            <canvas baseChart [data]="pnlChartData" [type]="'line'" [options]="lineOptions"></canvas>
          </div>
        </div>

        <h3>Snapshots</h3>
        <ul>
          <li *ngFor="let snapshot of snapshots()">
            {{ snapshot.capturedAtUtc | date:'medium' }}:
            {{ snapshot.totalMarketValueBase | number:'1.2-2' }} {{ snapshot.baseCurrency }}
            <span *ngIf="snapshot.isPartial">(partial)</span>
          </li>
        </ul>
      </article>
    </section>
  `,
  styles: `
    .page { padding: 1rem; display: grid; gap: 1rem; }
    .cards { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    article { border: 1px solid #d7d7d7; padding: 1rem; border-radius: 6px; }
    form { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 0.5rem; }
    input, select, button { padding: 0.45rem 0.6rem; }
    table { width: 100%; border-collapse: collapse; }
    th, td { border: 1px solid #d0d0d0; padding: 0.4rem; }
    .delete { background: #fff2f2; }
    .select { background: #f5f8ff; margin-right: 0.5rem; }
    .charts { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1rem; }
    @media (max-width: 1100px) {
      .cards, .charts { grid-template-columns: 1fr; }
    }
  `,
})
export class DashboardPage {
  private readonly formBuilder = inject(FormBuilder);

  readonly portfolios = signal<Portfolio[]>([]);
  readonly groups = signal<HoldingGroup[]>([]);
  readonly holdings = signal<Holding[]>([]);
  readonly snapshots = signal<Snapshot[]>([]);
  readonly analytics = signal<PortfolioAnalytics | null>(null);
  readonly selectedPortfolio = signal<Portfolio | null>(null);

  readonly portfolioForm = this.formBuilder.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    description: [''],
  });

  readonly groupForm = this.formBuilder.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
  });

  readonly holdingForm = this.formBuilder.group({
    symbol: ['', [Validators.required]],
    quantity: [1, [Validators.required, Validators.min(0.0001)]],
    averagePurchasePrice: [1, [Validators.required, Validators.min(0.01)]],
    currency: ['USD', [Validators.required]],
    groupId: [0, [Validators.required]],
  });

  valueChartData: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [{ data: [], label: 'Market Value' }] };
  pnlChartData: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [{ data: [], label: 'Unrealized P/L' }] };
  allocationChartData: ChartConfiguration<'doughnut'>['data'] = { labels: [], datasets: [{ data: [] }] };
  lineOptions: ChartConfiguration<'line'>['options'] = { responsive: true, maintainAspectRatio: false };

  constructor(
    private readonly portfolioService: PortfolioService,
    private readonly groupsService: GroupsService,
    private readonly analyticsService: AnalyticsService
  ) {
    this.loadPortfolios();
    this.loadGroups();
  }

  createPortfolio(): void {
    if (this.portfolioForm.invalid) {
      return;
    }
    const value = this.portfolioForm.getRawValue();
    this.portfolioService.createPortfolio(value.name ?? '', value.description ?? '').subscribe({
      next: () => {
        this.portfolioForm.reset({ name: '', description: '' });
        this.loadPortfolios();
      },
    });
  }

  deletePortfolio(id: number): void {
    this.portfolioService.deletePortfolio(id).subscribe({ next: () => this.loadPortfolios() });
  }

  createGroup(): void {
    if (this.groupForm.invalid) {
      return;
    }
    const value = this.groupForm.getRawValue();
    this.groupsService.createGroup(value.name ?? '').subscribe({
      next: () => {
        this.groupForm.reset({ name: '' });
        this.loadGroups();
      },
    });
  }

  deleteGroup(id: number): void {
    this.groupsService.deleteGroup(id).subscribe({
      next: () => {
        this.loadGroups();
        this.loadHoldingsAndAnalytics();
      },
    });
  }

  selectPortfolio(portfolio: Portfolio): void {
    this.selectedPortfolio.set(portfolio);
    this.loadHoldingsAndAnalytics();
  }

  addHolding(): void {
    const portfolio = this.selectedPortfolio();
    if (!portfolio || this.holdingForm.invalid) {
      return;
    }
    const value = this.holdingForm.getRawValue();
    this.portfolioService
      .addHolding(portfolio.id, {
        symbol: value.symbol ?? '',
        quantity: Number(value.quantity ?? 0),
        averagePurchasePrice: Number(value.averagePurchasePrice ?? 0),
        currency: value.currency ?? 'USD',
        groupId: Number(value.groupId ?? 0),
      })
      .subscribe({ next: () => this.loadHoldingsAndAnalytics() });
  }

  deleteHolding(holding: Holding): void {
    const portfolio = this.selectedPortfolio();
    if (!portfolio) {
      return;
    }
    this.portfolioService
      .deleteHolding(portfolio.id, holding.id)
      .subscribe({ next: () => this.loadHoldingsAndAnalytics() });
  }

  takeSnapshot(): void {
    const portfolio = this.selectedPortfolio();
    if (!portfolio) {
      return;
    }
    this.analyticsService.createSnapshot(portfolio.id).subscribe({
      next: () => this.loadHoldingsAndAnalytics(),
    });
  }

  private loadPortfolios(): void {
    this.portfolioService.getPortfolios().subscribe({
      next: (items) => {
        this.portfolios.set(items);
        if (!this.selectedPortfolio() && items.length > 0) {
          this.selectPortfolio(items[0]);
        }
      },
    });
  }

  private loadGroups(): void {
    this.groupsService.getGroups().subscribe({
      next: (items) => {
        this.groups.set(items);
        if (items.length > 0 && !this.holdingForm.value.groupId) {
          this.holdingForm.patchValue({ groupId: items[0].id });
        }
      },
    });
  }

  private loadHoldingsAndAnalytics(): void {
    const portfolio = this.selectedPortfolio();
    if (!portfolio) {
      return;
    }
    this.portfolioService.getHoldings(portfolio.id).subscribe({ next: (items) => this.holdings.set(items) });
    this.analyticsService.getPortfolioAnalytics(portfolio.id).subscribe({
      next: (data) => {
        this.analytics.set(data);
        this.allocationChartData = {
          labels: data.groupAllocations.map((entry) => entry.groupName),
          datasets: [{ data: data.groupAllocations.map((entry) => entry.marketValueBase) }],
        };
      },
    });
    this.analyticsService.getSnapshots(portfolio.id).subscribe({ next: (items) => this.snapshots.set(items) });
    this.analyticsService.getTimeSeries(portfolio.id).subscribe({
      next: (points) => this.updateTimeSeriesCharts(points),
    });
  }

  private updateTimeSeriesCharts(points: TimeSeriesPoint[]): void {
    const labels = points.map((point) => new Date(point.capturedAtUtc).toLocaleString());
    this.valueChartData = {
      labels,
      datasets: [{ label: 'Market Value', data: points.map((point) => point.totalMarketValueBase) }],
    };
    this.pnlChartData = {
      labels,
      datasets: [{ label: 'Unrealized P/L', data: points.map((point) => point.totalUnrealizedPnLBase) }],
    };
  }
}
