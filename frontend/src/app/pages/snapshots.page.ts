import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Portfolio } from '../models';
import { PortfolioWorkspaceService } from '../portfolio-workspace.service';

@Component({
  selector: 'app-snapshots-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="hf-page">
      <h1 class="h3 mb-3">Snapshots</h1>

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

      <div class="card hf-card" *ngIf="workspace.selectedPortfolio() as selected">
        <div class="card-body">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h2 class="h5 mb-0">{{ selected.name }} Snapshots</h2>
            <button class="btn btn-primary" (click)="takeSnapshot(selected.id)">Take Snapshot</button>
          </div>
          <div class="hf-table-wrap" *ngIf="workspace.snapshots().length > 0; else noSnapshots">
            <table class="table table-hover align-middle">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Market Value</th>
                  <th>Cost Basis</th>
                  <th>P/L</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let snapshot of workspace.snapshots()">
                  <td>{{ snapshot.capturedAtUtc | date:'medium' }}</td>
                  <td>{{ snapshot.totalMarketValueBase | number:'1.2-2' }} {{ snapshot.baseCurrency }}</td>
                  <td>{{ snapshot.totalCostBasisBase | number:'1.2-2' }} {{ snapshot.baseCurrency }}</td>
                  <td>{{ snapshot.totalUnrealizedPnLBase | number:'1.2-2' }} {{ snapshot.baseCurrency }}</td>
                  <td>
                    <span class="badge" [class.text-bg-warning]="snapshot.isPartial" [class.text-bg-success]="!snapshot.isPartial">
                      {{ snapshot.isPartial ? 'Partial' : 'Complete' }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <ng-template #noSnapshots>
            <div class="alert alert-info mb-0">No snapshots yet. Take one to build history.</div>
          </ng-template>
        </div>
      </div>
    </section>
  `,
})
export class SnapshotsPage {
  readonly workspace = inject(PortfolioWorkspaceService);

  constructor() {
    this.workspace.initialize();
  }

  selectPortfolio(portfolio: Portfolio): void {
    this.workspace.selectPortfolio(portfolio);
  }

  takeSnapshot(portfolioId: number): void {
    this.workspace.takeSnapshot(portfolioId);
  }
}
