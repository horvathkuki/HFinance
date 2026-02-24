import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { PortfolioAnalytics } from '../models';
import { PortfolioWorkspaceService } from '../portfolio-workspace.service';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  template: `
    <section class="hf-page">
      <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
        <h1 class="h3 mb-0">Dashboard</h1>
        <span class="badge text-bg-primary" *ngIf="workspace.selectedPortfolio() as selected">
          Active: {{ selected.name }}
        </span>
      </div>

      <div class="row mt-3" *ngIf="workspace.selectedPortfolio() && workspace.analytics() as metrics">
        <div class="col-12">
          <div class="card hf-card">
            <div class="card-body">
              <h2 class="h5 mb-3">Portfolio Summary</h2>
              <div class="row row-cols-1 row-cols-md-5 g-3">
                <div class="col"><div class="small text-muted">Base Currency</div><div class="fw-semibold">{{ metrics.baseCurrency }}</div></div>
                <div class="col"><div class="small text-muted">Holdings</div><div class="fw-semibold">{{ workspace.holdings().length }}</div></div>
                <div class="col"><div class="small text-muted">Market Value</div><div class="fw-semibold">{{ metrics.totalMarketValueBase | number:'1.2-2' }}</div></div>
                <div class="col"><div class="small text-muted">Unrealized P/L</div><div class="fw-semibold">{{ metrics.totalUnrealizedPnLBase | number:'1.2-2' }}</div></div>
                <div class="col"><div class="small text-muted">P/L %</div><div class="fw-semibold">{{ metrics.totalUnrealizedPnLPercent | number:'1.2-2' }}%</div></div>
              </div>
              <div class="alert alert-warning mt-3 mb-0" *ngIf="metrics.isPartial">
                Snapshot/analytics are partial. Missing symbols: {{ metrics.missingSymbolsCount }}.
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="row g-3 mt-1" *ngIf="workspace.selectedPortfolio() && workspace.analytics() as analytics">
        <div class="col-12 col-xl-6">
          <div class="card hf-card h-100">
            <div class="card-body">
              <h2 class="h5">Allocation by Group</h2>
              <canvas baseChart [data]="groupChartData(analytics)" [type]="'doughnut'"></canvas>
            </div>
          </div>
        </div>

        <div class="col-12 col-xl-6">
          <div class="card hf-card h-100">
            <div class="card-body">
              <h2 class="h5">Exposure by Currency</h2>
              <canvas baseChart [data]="currencyChartData(analytics)" [type]="'bar'"></canvas>
            </div>
          </div>
        </div>
      </div>
    </section>
  `,
})
export class DashboardPage {
  readonly workspace = inject(PortfolioWorkspaceService);

  constructor() {
    this.workspace.initialize();
  }

  groupChartData(analytics: PortfolioAnalytics): ChartConfiguration<'doughnut'>['data'] {
    return {
      labels: analytics.groupAllocations.map((item) => item.groupName),
      datasets: [{ data: analytics.groupAllocations.map((item) => item.marketValueBase) }],
    };
  }

  currencyChartData(analytics: PortfolioAnalytics): ChartConfiguration<'bar'>['data'] {
    return {
      labels: analytics.currencyExposures.map((item) => item.currency),
      datasets: [{ label: 'Market Value (Base)', data: analytics.currencyExposures.map((item) => item.marketValueBase) }],
    };
  }
}
