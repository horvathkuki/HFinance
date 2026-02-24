import { Injectable, signal } from '@angular/core';

export interface UiToastMessage {
  id: number;
  text: string;
  variant: 'success' | 'danger' | 'warning' | 'info';
}

@Injectable({ providedIn: 'root' })
export class UiToastService {
  readonly messages = signal<UiToastMessage[]>([]);
  private idCounter = 1;

  show(text: string, variant: UiToastMessage['variant'] = 'info'): void {
    const id = this.idCounter++;
    this.messages.update((items) => [...items, { id, text, variant }]);
  }

  remove(id: number): void {
    this.messages.update((items) => items.filter((item) => item.id !== id));
  }
}
