import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { Portfolio, PortfolioAnalytics } from '../models';
import { PortfolioWorkspaceService } from '../portfolio-workspace.service';

Chart.register(...registerables);

@Component({
  selector: 'app-allocations-page',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  template: `
    <section class="hf-page">
      <h1 class="h3 mb-3">Allocations</h1>

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

      <div class="row g-3" *ngIf="workspace.selectedPortfolio() && workspace.analytics() as analytics">
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
export class AllocationsPage {
  readonly workspace = inject(PortfolioWorkspaceService);

  constructor() {
    this.workspace.initialize();
  }

  selectPortfolio(portfolio: Portfolio): void {
    this.workspace.selectPortfolio(portfolio);
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
