import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="modal-header">
      <h5 class="modal-title">{{ title }}</h5>
      <button type="button" class="btn-close" aria-label="Close" (click)="activeModal.dismiss('cancel')"></button>
    </div>
    <div class="modal-body">
      <p class="mb-0">{{ body }}</p>
    </div>
    <div class="modal-footer">
      <button type="button" class="btn btn-outline-secondary" (click)="activeModal.dismiss('cancel')">Cancel</button>
      <button type="button" class="btn" [ngClass]="confirmButtonClass" (click)="activeModal.close(true)">
        {{ confirmLabel }}
      </button>
    </div>
  `,
})
export class ConfirmDialogComponent {
  @Input() title = 'Confirm';
  @Input() body = 'Are you sure?';
  @Input() confirmLabel = 'Confirm';
  @Input() confirmButtonClass = 'btn-danger';

  constructor(public readonly activeModal: NgbActiveModal) {}
}
