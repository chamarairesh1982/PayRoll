import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TaxSlab } from '../../models/tax-rule-set.model';

@Component({
  selector: 'app-tax-slabs-editor',
  templateUrl: './tax-slabs-editor.component.html',
  styleUrls: ['./tax-slabs-editor.component.scss'],
})
export class TaxSlabsEditorComponent {
  @Input() slabs: TaxSlab[] = [];
  @Output() slabsChange = new EventEmitter<TaxSlab[]>();

  addSlab(): void {
    const nextOrder = this.slabs.length > 0 ? Math.max(...this.slabs.map(s => s.order)) + 1 : 1;
    this.slabs = [
      ...this.slabs,
      {
        fromAmount: 0,
        toAmount: null,
        ratePercent: 0,
        order: nextOrder,
      },
    ];
    this.emitChange();
  }

  removeSlab(index: number): void {
    this.slabs = this.slabs.filter((_, i) => i !== index).map((slab, i) => ({ ...slab, order: i + 1 }));
    this.emitChange();
  }

  moveUp(index: number): void {
    if (index === 0) {
      return;
    }
    const updated = [...this.slabs];
    [updated[index - 1], updated[index]] = [updated[index], updated[index - 1]];
    this.slabs = updated.map((slab, i) => ({ ...slab, order: i + 1 }));
    this.emitChange();
  }

  moveDown(index: number): void {
    if (index >= this.slabs.length - 1) {
      return;
    }
    const updated = [...this.slabs];
    [updated[index + 1], updated[index]] = [updated[index], updated[index + 1]];
    this.slabs = updated.map((slab, i) => ({ ...slab, order: i + 1 }));
    this.emitChange();
  }

  onChange(): void {
    this.slabs = this.slabs.map((slab, index) => ({
      ...slab,
      order: index + 1,
      fromAmount: Number(slab.fromAmount),
      toAmount: slab.toAmount === null || slab.toAmount === undefined || slab.toAmount === '' ? null : Number(slab.toAmount),
      ratePercent: Number(slab.ratePercent),
    }));
    this.emitChange();
  }

  private emitChange(): void {
    this.slabsChange.emit([...this.slabs]);
  }
}
