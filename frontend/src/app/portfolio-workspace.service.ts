import { Injectable, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AnalyticsService } from './analytics.service';
import { GroupsService } from './groups.service';
import { PortfolioService } from './portfolio.service';
import { Holding, HoldingGroup, Portfolio, PortfolioAnalytics, Snapshot, TimeSeriesPoint } from './models';
import { UiToastService } from './ui-toast.service';

@Injectable({ providedIn: 'root' })
export class PortfolioWorkspaceService {
  readonly portfolios = signal<Portfolio[]>([]);
  readonly groups = signal<HoldingGroup[]>([]);
  readonly holdings = signal<Holding[]>([]);
  readonly snapshots = signal<Snapshot[]>([]);
  readonly analytics = signal<PortfolioAnalytics | null>(null);
  readonly timeSeries = signal<TimeSeriesPoint[]>([]);
  readonly selectedPortfolio = signal<Portfolio | null>(null);

  constructor(
    private readonly portfolioService: PortfolioService,
    private readonly groupsService: GroupsService,
    private readonly analyticsService: AnalyticsService,
    private readonly toastService: UiToastService
  ) {}

  initialize(): void {
    this.loadPortfolios();
    this.loadGroups();
  }

  createPortfolio(name: string, description?: string): void {
    this.portfolioService.createPortfolio(name, description).subscribe({
      next: () => {
        this.toastService.show('Portfolio created.', 'success');
        this.loadPortfolios();
      },
      error: () => this.toastService.show('Could not create portfolio.', 'danger'),
    });
  }

  deletePortfolio(id: number): void {
    this.portfolioService.deletePortfolio(id).subscribe({
      next: () => {
        this.toastService.show('Portfolio deleted.', 'warning');
        const selected = this.selectedPortfolio();
        if (selected?.id === id) {
          this.selectedPortfolio.set(null);
        }
        this.loadPortfolios();
      },
      error: () => this.toastService.show('Could not delete portfolio.', 'danger'),
    });
  }

  createGroup(name: string, description?: string): void {
    this.groupsService.createGroup(name, description).subscribe({
      next: () => {
        this.toastService.show('Group created.', 'success');
        this.loadGroups();
      },
      error: () => this.toastService.show('Could not create group.', 'danger'),
    });
  }

  deleteGroup(id: number): void {
    this.groupsService.deleteGroup(id).subscribe({
      next: () => {
        this.toastService.show('Group deleted. Holdings moved to Uncategorized.', 'warning');
        this.loadGroups();
        this.reloadCurrentPortfolioData();
      },
      error: () => this.toastService.show('Could not delete group.', 'danger'),
    });
  }

  addHolding(portfolioId: number, payload: {
    symbol: string;
    quantity: number;
    averagePurchasePrice: number;
    currency: string;
    groupId: number;
  }): void {
    this.portfolioService.addHolding(portfolioId, payload).subscribe({
      next: () => {
        this.toastService.show('Holding added.', 'success');
        this.reloadCurrentPortfolioData();
      },
      error: () => this.toastService.show('Could not add holding.', 'danger'),
    });
  }

  deleteHolding(portfolioId: number, holdingId: number): void {
    this.portfolioService.deleteHolding(portfolioId, holdingId).subscribe({
      next: () => {
        this.toastService.show('Holding deleted.', 'warning');
        this.reloadCurrentPortfolioData();
      },
      error: () => this.toastService.show('Could not delete holding.', 'danger'),
    });
  }

  takeSnapshot(portfolioId: number): void {
    this.analyticsService.createSnapshot(portfolioId).subscribe({
      next: () => {
        this.toastService.show('Snapshot created.', 'success');
        this.reloadCurrentPortfolioData();
      },
      error: () => this.toastService.show('Could not create snapshot.', 'danger'),
    });
  }

  selectPortfolio(portfolio: Portfolio): void {
    this.selectedPortfolio.set(portfolio);
    this.reloadCurrentPortfolioData();
  }

  private loadPortfolios(): void {
    this.portfolioService.getPortfolios().subscribe({
      next: (items) => {
        this.portfolios.set(items);
        const current = this.selectedPortfolio();
        if (!current && items.length > 0) {
          this.selectPortfolio(items[0]);
          return;
        }

        if (current) {
          const refreshedCurrent = items.find((item) => item.id === current.id) ?? null;
          this.selectedPortfolio.set(refreshedCurrent);
          if (refreshedCurrent) {
            this.reloadCurrentPortfolioData();
          } else {
            this.clearPortfolioScopedData();
          }
        } else if (items.length === 0) {
          this.clearPortfolioScopedData();
        }
      },
    });
  }

  private loadGroups(): void {
    this.groupsService.getGroups().subscribe({
      next: (items) => this.groups.set(items),
    });
  }

  private reloadCurrentPortfolioData(): void {
    const portfolio = this.selectedPortfolio();
    if (!portfolio) {
      this.clearPortfolioScopedData();
      return;
    }

    forkJoin({
      holdings: this.portfolioService.getHoldings(portfolio.id),
      analytics: this.analyticsService.getPortfolioAnalytics(portfolio.id),
      snapshots: this.analyticsService.getSnapshots(portfolio.id),
      timeSeries: this.analyticsService.getTimeSeries(portfolio.id),
    }).subscribe({
      next: (result) => {
        this.holdings.set(result.holdings);
        this.analytics.set(result.analytics);
        this.snapshots.set(result.snapshots);
        this.timeSeries.set(result.timeSeries);
      },
      error: () => this.toastService.show('Could not load portfolio data.', 'danger'),
    });
  }

  private clearPortfolioScopedData(): void {
    this.holdings.set([]);
    this.analytics.set(null);
    this.snapshots.set([]);
    this.timeSeries.set([]);
  }
}
