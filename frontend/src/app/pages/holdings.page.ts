import { Component, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Holding, Portfolio } from '../models';
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
              <input class="form-control" type="date" formControlName="purchaseDate" />
            </div>
            <div class="col-12 col-md-2">
              <button class="btn btn-primary w-100" type="submit" [disabled]="holdingForm.invalid">
                {{ isEditMode() ? 'Save' : 'Add' }}
              </button>
            </div>
            <div class="col-12 col-md-2" *ngIf="isEditMode()">
              <button class="btn btn-outline-secondary w-100" type="button" (click)="cancelEdit()">Cancel</button>
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
                  <th>Purchase Date</th>
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
                  <td>{{ holding.purchaseDate | date:'yyyy-MM-dd' }}</td>
                  <td>
                    <div class="d-flex gap-2">
                      <button class="btn btn-sm btn-outline-primary" (click)="startEdit(holding)" title="Edit holding" aria-label="Edit holding">
                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16">
                          <path d="M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708L5.207 14.5H2a1 1 0 0 1-1-1v-3.207zM11.207 2 2 11.207V14h2.793L14 4.793z"/>
                        </svg>
                      </button>
                      <button class="btn btn-sm btn-outline-danger" (click)="confirmDeleteHolding(holding.id, holding.symbol)" title="Delete holding" aria-label="Delete holding">
                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16">
                          <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5m2.5 0A.5.5 0 0 1 8.5 6v6a.5.5 0 0 1-1 0V6A.5.5 0 0 1 8 5.5m3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0z"/>
                          <path d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 1 1 0-2H5.5l1-1h3l1 1h3a1 1 0 0 1 1 1M4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4z"/>
                        </svg>
                      </button>
                    </div>
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
  readonly editingHoldingId = signal<number | null>(null);
  readonly isEditMode = signal(false);

  readonly holdingForm = this.formBuilder.group({
    symbol: ['', [Validators.required]],
    quantity: [1, [Validators.required, Validators.min(0.0001)]],
    averagePurchasePrice: [1, [Validators.required, Validators.min(0.01)]],
    currency: ['USD', [Validators.required]],
    groupId: [0, [Validators.required, Validators.min(1)]],
    purchaseDate: [''],
  });

  constructor() {
    this.workspace.initialize();
    this.prefillDefaultGroup();
    effect(() => {
      const groups = this.workspace.groups();
      if (!this.isEditMode() && groups.length > 0) {
        const currentGroupId = Number(this.holdingForm.controls.groupId.value ?? 0);
        if (currentGroupId <= 0) {
          this.holdingForm.patchValue({ groupId: groups[0].id });
        }
      }
    });
  }

  selectPortfolio(portfolio: Portfolio): void {
    this.workspace.selectPortfolio(portfolio);
    this.resetForm();
    this.prefillDefaultGroup();
  }

  addHolding(): void {
    const selected = this.workspace.selectedPortfolio();
    if (!selected || this.holdingForm.invalid) {
      return;
    }
    const value = this.holdingForm.getRawValue();
    const payload = {
      symbol: (value.symbol ?? '').trim().toUpperCase(),
      quantity: Number(value.quantity ?? 0),
      averagePurchasePrice: Number(value.averagePurchasePrice ?? 0),
      currency: value.currency ?? 'USD',
      groupId: Number(value.groupId ?? 0),
      purchaseDate: value.purchaseDate || undefined,
    };

    const editingId = this.editingHoldingId();
    if (this.isEditMode() && editingId !== null) {
      this.workspace.updateHolding(selected.id, editingId, payload);
    } else {
      this.workspace.addHolding(selected.id, payload);
    }

    this.resetForm();
  }

  deleteHolding(holdingId: number): void {
    const selected = this.workspace.selectedPortfolio();
    if (!selected) {
      return;
    }
    this.workspace.deleteHolding(selected.id, holdingId);
    if (this.editingHoldingId() === holdingId) {
      this.resetForm();
    }
  }

  startEdit(holding: Holding): void {
    this.isEditMode.set(true);
    this.editingHoldingId.set(holding.id);
    this.holdingForm.patchValue({
      symbol: holding.symbol,
      quantity: holding.quantity,
      averagePurchasePrice: holding.averagePurchasePrice,
      currency: holding.currency,
      groupId: holding.groupId,
      purchaseDate: this.toDateInput(holding.purchaseDate),
    });
  }

  cancelEdit(): void {
    this.resetForm();
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

  private resetForm(): void {
    this.isEditMode.set(false);
    this.editingHoldingId.set(null);
    this.holdingForm.patchValue({
      symbol: '',
      quantity: 1,
      averagePurchasePrice: 1,
      currency: 'USD',
      groupId: this.workspace.groups()[0]?.id ?? 0,
      purchaseDate: '',
    });
  }

  private toDateInput(value: string): string {
    if (!value) {
      return '';
    }
    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) {
      return '';
    }
    return parsed.toISOString().slice(0, 10);
  }

  private prefillDefaultGroup(): void {
    const firstGroup = this.workspace.groups()[0];
    if (firstGroup) {
      this.holdingForm.patchValue({ groupId: firstGroup.id });
    }
  }
}
