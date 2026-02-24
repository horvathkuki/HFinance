import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgbDropdownModule, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Portfolio } from '../models';
import { PortfolioWorkspaceService } from '../portfolio-workspace.service';
import { ConfirmDialogComponent } from '../shared/confirm-dialog.component';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgbDropdownModule],
  template: `
    <section class="hf-page">
      <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
        <h1 class="h3 mb-0">Dashboard</h1>
        <span class="badge text-bg-primary" *ngIf="workspace.selectedPortfolio() as selected">
          Active: {{ selected.name }}
        </span>
      </div>

      <div class="row g-3">
        <div class="col-12 col-xl-6">
          <div class="card hf-card h-100">
            <div class="card-body">
              <h2 class="h5 mb-3">Portfolios</h2>
              <form [formGroup]="portfolioForm" (ngSubmit)="createPortfolio()" class="row g-2 mb-3">
                <div class="col-12 col-md-4">
                  <input class="form-control" type="text" placeholder="Portfolio name" formControlName="name" />
                </div>
                <div class="col-12 col-md-5">
                  <input class="form-control" type="text" placeholder="Description" formControlName="description" />
                </div>
                <div class="col-12 col-md-3">
                  <button class="btn btn-primary w-100" type="submit" [disabled]="portfolioForm.invalid">Create</button>
                </div>
              </form>
              <div class="list-group">
                <div class="list-group-item d-flex justify-content-between align-items-center" *ngFor="let portfolio of workspace.portfolios()">
                  <button class="btn btn-link p-0 text-decoration-none fw-semibold" (click)="selectPortfolio(portfolio)">
                    {{ portfolio.name }}
                  </button>
                  <div ngbDropdown container="body">
                    <button class="btn btn-sm btn-outline-secondary" ngbDropdownToggle>Actions</button>
                    <div ngbDropdownMenu>
                      <button ngbDropdownItem (click)="confirmDeletePortfolio(portfolio)">Delete</button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-12 col-xl-6">
          <div class="card hf-card h-100">
            <div class="card-body">
              <h2 class="h5 mb-3">Groups</h2>
              <form [formGroup]="groupForm" (ngSubmit)="createGroup()" class="row g-2 mb-3">
                <div class="col-12 col-md-8">
                  <input class="form-control" type="text" placeholder="Group name" formControlName="name" />
                </div>
                <div class="col-12 col-md-4">
                  <button class="btn btn-primary w-100" type="submit" [disabled]="groupForm.invalid">Add Group</button>
                </div>
              </form>
              <div class="list-group">
                <div class="list-group-item d-flex justify-content-between align-items-center" *ngFor="let group of workspace.groups()">
                  <span>{{ group.name }}</span>
                  <button class="btn btn-sm btn-outline-danger" (click)="confirmDeleteGroup(group.id, group.name)">Delete</button>
                </div>
              </div>
            </div>
          </div>
        </div>
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
    </section>
  `,
})
export class DashboardPage {
  private readonly formBuilder = inject(FormBuilder);
  private readonly modalService = inject(NgbModal);
  readonly workspace = inject(PortfolioWorkspaceService);

  readonly portfolioForm = this.formBuilder.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    description: [''],
  });

  readonly groupForm = this.formBuilder.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
  });

  constructor() {
    this.workspace.initialize();
  }

  createPortfolio(): void {
    if (this.portfolioForm.invalid) {
      return;
    }
    const value = this.portfolioForm.getRawValue();
    this.workspace.createPortfolio(value.name ?? '', value.description ?? '');
    this.portfolioForm.reset({ name: '', description: '' });
  }

  deletePortfolio(id: number): void {
    this.workspace.deletePortfolio(id);
  }

  createGroup(): void {
    if (this.groupForm.invalid) {
      return;
    }
    const value = this.groupForm.getRawValue();
    this.workspace.createGroup(value.name ?? '');
    this.groupForm.reset({ name: '' });
  }

  deleteGroup(id: number): void {
    this.workspace.deleteGroup(id);
  }

  selectPortfolio(portfolio: Portfolio): void {
    this.workspace.selectPortfolio(portfolio);
  }

  confirmDeletePortfolio(portfolio: Portfolio): void {
    const modalRef = this.modalService.open(ConfirmDialogComponent, { centered: true });
    modalRef.componentInstance.title = 'Delete Portfolio';
    modalRef.componentInstance.body = `Delete portfolio "${portfolio.name}"? This removes all related holdings and snapshots.`;
    modalRef.componentInstance.confirmLabel = 'Delete';
    modalRef.componentInstance.confirmButtonClass = 'btn-danger';
    modalRef.result.then((confirmed: boolean) => {
      if (confirmed) {
        this.deletePortfolio(portfolio.id);
      }
    }).catch(() => {});
  }

  confirmDeleteGroup(groupId: number, groupName: string): void {
    const modalRef = this.modalService.open(ConfirmDialogComponent, { centered: true });
    modalRef.componentInstance.title = 'Delete Group';
    modalRef.componentInstance.body = `Delete group "${groupName}"? Holdings will be moved to Uncategorized.`;
    modalRef.componentInstance.confirmLabel = 'Delete';
    modalRef.componentInstance.confirmButtonClass = 'btn-danger';
    modalRef.result.then((confirmed: boolean) => {
      if (confirmed) {
        this.deleteGroup(groupId);
      }
    }).catch(() => {});
  }
}
