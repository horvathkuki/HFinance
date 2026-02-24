import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Portfolio } from '../models';
import { PortfolioWorkspaceService } from '../portfolio-workspace.service';
import { ConfirmDialogComponent } from '../shared/confirm-dialog.component';

@Component({
  selector: 'app-holdings-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="hf-page">
      <h1 class="h3 mb-3">Holdings</h1>

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

      <div class="card hf-card" *ngIf="workspace.selectedPortfolio() as currentPortfolio">
        <div class="card-body">
          <h2 class="h5 mb-3">{{ currentPortfolio.name }} - Holdings</h2>

          <form [formGroup]="holdingForm" (ngSubmit)="addHolding()" class="row g-2 mb-3">
            <div class="col-12 col-md-2">
              <input class="form-control" type="text" placeholder="Symbol" formControlName="symbol" [class.is-invalid]="holdingInvalid('symbol')" />
            </div>
            <div class="col-12 col-md-2">
              <input class="form-control" type="number" step="0.0001" placeholder="Quantity" formControlName="quantity" [class.is-invalid]="holdingInvalid('quantity')" />
            </div>
            <div class="col-12 col-md-2">
              <input class="form-control" type="number" step="0.01" placeholder="Avg price" formControlName="averagePurchasePrice" [class.is-invalid]="holdingInvalid('averagePurchasePrice')" />
            </div>
            <div class="col-12 col-md-2">
              <select class="form-select" formControlName="currency">
                <option value="EUR">EUR</option>
                <option value="USD">USD</option>
                <option value="RON">RON</option>
              </select>
            </div>
            <div class="col-12 col-md-2">
              <select class="form-select" formControlName="groupId">
                <option *ngFor="let group of workspace.groups()" [value]="group.id">{{ group.name }}</option>
              </select>
            </div>
            <div class="col-12 col-md-2">
              <button class="btn btn-primary w-100" type="submit" [disabled]="holdingForm.invalid">Add</button>
            </div>
          </form>

          <div class="hf-table-wrap" *ngIf="workspace.holdings().length > 0; else emptyState">
            <table class="table table-striped table-hover align-middle">
              <thead>
                <tr>
                  <th>Symbol</th>
                  <th>Quantity</th>
                  <th>Avg Price</th>
                  <th>Currency</th>
                  <th>Group</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let holding of workspace.holdings()">
                  <td class="fw-semibold">{{ holding.symbol }}</td>
                  <td>{{ holding.quantity }}</td>
                  <td>{{ holding.averagePurchasePrice }}</td>
                  <td>{{ holding.currency }}</td>
                  <td>{{ holding.groupName }}</td>
                  <td>
                    <button class="btn btn-sm btn-outline-danger" (click)="confirmDeleteHolding(holding.id, holding.symbol)">Delete</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <ng-template #emptyState>
            <div class="alert alert-info mb-0">No holdings yet for this portfolio.</div>
          </ng-template>
        </div>
      </div>
    </section>
  `,
})
export class HoldingsPage {
  private readonly formBuilder = inject(FormBuilder);
  private readonly modalService = inject(NgbModal);
  readonly workspace = inject(PortfolioWorkspaceService);

  readonly holdingForm = this.formBuilder.group({
    symbol: ['', [Validators.required]],
    quantity: [1, [Validators.required, Validators.min(0.0001)]],
    averagePurchasePrice: [1, [Validators.required, Validators.min(0.01)]],
    currency: ['USD', [Validators.required]],
    groupId: [0, [Validators.required]],
  });

  constructor() {
    this.workspace.initialize();
    this.prefillDefaultGroup();
  }

  selectPortfolio(portfolio: Portfolio): void {
    this.workspace.selectPortfolio(portfolio);
    this.prefillDefaultGroup();
  }

  addHolding(): void {
    const selected = this.workspace.selectedPortfolio();
    if (!selected || this.holdingForm.invalid) {
      return;
    }
    const value = this.holdingForm.getRawValue();
    this.workspace.addHolding(selected.id, {
      symbol: value.symbol ?? '',
      quantity: Number(value.quantity ?? 0),
      averagePurchasePrice: Number(value.averagePurchasePrice ?? 0),
      currency: value.currency ?? 'USD',
      groupId: Number(value.groupId ?? 0),
    });
    this.holdingForm.patchValue({ symbol: '', quantity: 1, averagePurchasePrice: 1 });
  }

  deleteHolding(holdingId: number): void {
    const selected = this.workspace.selectedPortfolio();
    if (!selected) {
      return;
    }
    this.workspace.deleteHolding(selected.id, holdingId);
  }

  confirmDeleteHolding(holdingId: number, symbol: string): void {
    const modalRef = this.modalService.open(ConfirmDialogComponent, { centered: true });
    modalRef.componentInstance.title = 'Delete Holding';
    modalRef.componentInstance.body = `Delete holding "${symbol}" from this portfolio?`;
    modalRef.componentInstance.confirmLabel = 'Delete';
    modalRef.componentInstance.confirmButtonClass = 'btn-danger';
    modalRef.result.then((confirmed: boolean) => {
      if (confirmed) {
        this.deleteHolding(holdingId);
      }
    }).catch(() => {});
  }

  holdingInvalid(controlName: string): boolean {
    const control = this.holdingForm.controls[controlName as keyof typeof this.holdingForm.controls];
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  private prefillDefaultGroup(): void {
    const firstGroup = this.workspace.groups()[0];
    if (firstGroup) {
      this.holdingForm.patchValue({ groupId: firstGroup.id });
    }
  }
}
