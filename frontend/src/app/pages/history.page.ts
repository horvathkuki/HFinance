import { Component, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { Portfolio } from '../models';
import { PortfolioWorkspaceService } from '../portfolio-workspace.service';

Chart.register(...registerables);

@Component({
  selector: 'app-history-page',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  template: `
    <section class="hf-page">
      <h1 class="h3 mb-3">History</h1>

      <div class="card hf-card mb-3">
        <div class="card-body">
          <h2 class="h5">Select Portfolio</h2>
          <div class="d-flex gap-2 flex-wrap">
            <button class="btn btn-outline-primary btn-sm" *ngFor="let portfolio of workspace.portfolios()" (click)="selectPortfolio(portfolio)">
              {{ portfolio.name }}
            </button>
          </div>
        </div>
      </div>

      <div class="row g-3" *ngIf="workspace.selectedPortfolio()">
        <div class="col-12">
          <div class="card hf-card">
            <div class="card-body">
              <h2 class="h5">Market Value Over Time</h2>
              <canvas baseChart [data]="marketValueChartData" [type]="'line'" [options]="lineOptions"></canvas>
            </div>
          </div>
        </div>

        <div class="col-12">
          <div class="card hf-card">
            <div class="card-body">
              <h2 class="h5">Unrealized P/L Over Time</h2>
              <canvas baseChart [data]="pnlChartData" [type]="'line'" [options]="lineOptions"></canvas>
            </div>
          </div>
        </div>
      </div>
    </section>
  `,
})
export class HistoryPage {
  readonly workspace = inject(PortfolioWorkspaceService);

  marketValueChartData: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [{ label: 'Market Value', data: [] }] };
  pnlChartData: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [{ label: 'Unrealized P/L', data: [] }] };
  lineOptions: ChartConfiguration<'line'>['options'] = { responsive: true, maintainAspectRatio: false };

  constructor() {
    this.workspace.initialize();
    effect(() => {
      const points = this.workspace.timeSeries();
      const labels = points.map((point) => new Date(point.capturedAtUtc).toLocaleString());
      this.marketValueChartData = {
        labels,
        datasets: [{ label: 'Market Value', data: points.map((point) => point.totalMarketValueBase) }],
      };
      this.pnlChartData = {
        labels,
        datasets: [{ label: 'Unrealized P/L', data: points.map((point) => point.totalUnrealizedPnLBase) }],
      };
    });
  }

  selectPortfolio(portfolio: Portfolio): void {
    this.workspace.selectPortfolio(portfolio);
  }
}
