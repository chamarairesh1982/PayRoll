import { Component, OnDestroy, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormGroup,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MessageService } from 'primeng/api';
import { Subject, takeUntil } from 'rxjs';
import { TaxScheme, TaxRelief } from '../../models/tax-scheme.model';
import { TaxConfigService } from '../../services/tax-config.service';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

@Component({
  selector: 'app-tax-config-page',
  templateUrl: './tax-config-page.component.html',
  styleUrls: ['./tax-config-page.component.scss'],
  providers: [MessageService],
})
export class TaxConfigPageComponent implements OnInit, OnDestroy {
  schemes: TaxScheme[] = [];
  isLoading = false;

  columns: DataTableColumn<TaxScheme>[] = [
    { field: 'name', header: 'Fiscal Scheme', sortable: true, minWidth: '200px' },
    { field: 'effectiveFrom', header: 'Statutory Window', minWidth: '250px' },
    { field: 'slabs', header: 'Complexity', minWidth: '150px' },
    { field: 'reliefs', header: 'Active Reliefs', minWidth: '200px' },
    { field: 'isActive', header: 'Governance Status', sortable: true, type: 'status', minWidth: '150px' },
  ];
  dialogVisible = false;
  dialogMode: 'create' | 'edit' | 'clone' = 'create';
  activeScheme: TaxScheme | null = null;

  form: FormGroup = this.fb.group({
    id: [''],
    name: ['', Validators.required],
    effectiveFrom: ['', Validators.required],
    effectiveTo: [''],
    isActive: [true],
    slabs: this.fb.array([], this.validateSlabs.bind(this)),
    reliefs: this.fb.array([]),
  });

  reliefTypes = [
    { label: 'Income relief', value: 'IncomeRelief' },
    { label: 'Tax rebate', value: 'TaxRebate' },
  ];

  reliefFrequencies = [
    { label: 'Monthly', value: 'Monthly' },
    { label: 'Annual', value: 'Annual' },
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private taxConfigService: TaxConfigService,
    private messageService: MessageService,
  ) { }

  ngOnInit(): void {
    this.taxConfigService
      .getSchemes()
      .pipe(takeUntil(this.destroy$))
      .subscribe(schemes => (this.schemes = schemes));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get slabs(): FormArray {
    return this.form.get('slabs') as FormArray;
  }

  get reliefs(): FormArray {
    return this.form.get('reliefs') as FormArray;
  }

  openCreate(): void {
    this.dialogMode = 'create';
    this.activeScheme = null;
    this.resetForm();
    this.dialogVisible = true;
  }

  openEdit(scheme: TaxScheme): void {
    this.dialogMode = 'edit';
    this.activeScheme = scheme;
    this.resetForm(scheme);
    this.dialogVisible = true;
  }

  openClone(scheme: TaxScheme): void {
    this.dialogMode = 'clone';
    this.activeScheme = scheme;
    this.resetForm({
      ...scheme,
      id: '',
      name: `${scheme.name} (Copy)`,
      effectiveFrom: scheme.effectiveFrom,
      effectiveTo: scheme.effectiveTo,
      isActive: false,
    });
    this.dialogVisible = true;
  }

  addSlab(): void {
    this.slabs.push(this.createSlab());
  }

  removeSlab(index: number): void {
    this.slabs.removeAt(index);
    this.slabs.updateValueAndValidity();
  }

  addRelief(): void {
    this.reliefs.push(this.createReliefGroup(this.taxConfigService.createRelief()));
  }

  removeRelief(index: number): void {
    this.reliefs.removeAt(index);
  }

  toggleOpenEnded(index: number): void {
    const slabGroup = this.slabs.at(index) as FormGroup;
    const openEnded = slabGroup.get('openEnded')?.value;
    if (openEnded) {
      slabGroup.get('to')?.setValue(null);
      slabGroup.get('to')?.disable({ emitEvent: false });
    } else {
      slabGroup.get('to')?.enable({ emitEvent: false });
    }
    this.slabs.updateValueAndValidity();
  }

  saveScheme(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    const scheme = this.buildSchemeFromForm();

    if (this.dialogMode === 'edit' && this.activeScheme) {
      this.taxConfigService.updateScheme(scheme);
      this.messageService.add({
        severity: 'success',
        summary: 'Tax scheme updated',
        detail: `${scheme.name} was saved successfully.`,
      });
    } else if (this.dialogMode === 'clone' && this.activeScheme) {
      this.taxConfigService.cloneScheme(scheme, this.activeScheme);
      this.messageService.add({
        severity: 'success',
        summary: 'Tax scheme cloned',
        detail: `${scheme.name} was created from ${this.activeScheme.name}.`,
      });
    } else {
      this.taxConfigService.createScheme(scheme);
      this.messageService.add({
        severity: 'success',
        summary: 'Tax scheme created',
        detail: `${scheme.name} was added successfully.`,
      });
    }

    this.dialogVisible = false;
  }

  private resetForm(scheme?: Partial<TaxScheme>): void {
    this.form.reset({
      id: scheme?.id ?? '',
      name: scheme?.name ?? '',
      effectiveFrom: scheme?.effectiveFrom ? new Date(`${scheme.effectiveFrom}T00:00:00`) : null,
      effectiveTo: scheme?.effectiveTo ? new Date(`${scheme.effectiveTo}T00:00:00`) : null,
      isActive: scheme?.isActive ?? true,
    });

    this.slabs.clear();
    const slabs = scheme?.slabs ?? [];
    if (slabs.length === 0) {
      this.addSlab();
    } else {
      slabs.forEach(slab => this.slabs.push(this.createSlab(slab)));
    }

    this.reliefs.clear();
    (scheme?.reliefs ?? []).forEach(relief => this.reliefs.push(this.createReliefGroup(relief)));
  }

  private buildSchemeFromForm(): TaxScheme {
    const raw = this.form.getRawValue();
    const slabs = raw.slabs.map((slab: any) => ({
      from: Number(slab.from || 0),
      to: slab.openEnded ? null : Number(slab.to),
      rate: Number(slab.rate || 0),
    }));

    const reliefs: TaxRelief[] = raw.reliefs.map((relief: any) => ({
      id: relief.id,
      description: relief.description,
      amount: Number(relief.amount || 0),
      reliefType: relief.reliefType,
      frequency: relief.frequency,
    }));

    return {
      id: raw.id || this.taxConfigService.createSchemeId(),
      name: raw.name,
      effectiveFrom: this.toIsoDate(raw.effectiveFrom),
      effectiveTo: raw.effectiveTo ? this.toIsoDate(raw.effectiveTo) : null,
      isActive: raw.isActive,
      slabs,
      reliefs,
    };
  }

  private createSlab(slab?: TaxScheme['slabs'][number]): FormGroup {
    const group = this.fb.group({
      from: [slab?.from ?? 0, [Validators.required, Validators.min(0)]],
      to: [slab?.to ?? null, [Validators.min(0)]],
      rate: [slab?.rate ?? 0, [Validators.required, Validators.min(0), Validators.max(100)]],
      openEnded: [slab?.to == null],
    });
    if (slab?.to == null) {
      group.get('to')?.disable({ emitEvent: false });
    }
    return group;
  }

  private createReliefGroup(relief: TaxRelief): FormGroup {
    return this.fb.group({
      id: [relief.id],
      description: [relief.description, Validators.required],
      amount: [relief.amount, [Validators.required, Validators.min(0)]],
      reliefType: [relief.reliefType, Validators.required],
      frequency: [relief.frequency, Validators.required],
    });
  }

  private validateSlabs(control: AbstractControl): ValidationErrors | null {
    const slabs = (control.value as Array<{ from: number; to: number | null; openEnded?: boolean }>) || [];

    if (slabs.length === 0) {
      return { slabError: 'At least one slab is required.' };
    }

    let previousTo: number | null = null;

    for (let index = 0; index < slabs.length; index += 1) {
      const slab = slabs[index];
      const from = Number(slab.from);
      const to = slab.to === null || slab.to === undefined ? null : Number(slab.to);

      if (index === 0 && from !== 0) {
        return { slabError: 'The first slab must start at 0.' };
      }

      if (previousTo !== null && from !== previousTo) {
        return { slabError: 'Slab ranges must be continuous with no gaps or overlaps.' };
      }

      if (to !== null && to <= from) {
        return { slabError: 'Each slab must end after its start value.' };
      }

      if (to === null && index < slabs.length - 1) {
        return { slabError: 'Only the last slab can be open-ended.' };
      }

      previousTo = to;
    }

    return null;
  }

  private toIsoDate(value: Date | string | null): string {
    if (!value) {
      return '';
    }
    if (value instanceof Date) {
      return value.toISOString().split('T')[0];
    }
    return value;
  }
}
