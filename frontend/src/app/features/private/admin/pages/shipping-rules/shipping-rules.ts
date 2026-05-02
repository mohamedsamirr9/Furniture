import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ShippingRulesService } from '../../../../../core/services/shipping-rules.service';
import { CategoryService } from '../../../../../core/services/category.service';
import { ShippingRule, ShippingRuleCreateUpdate } from '../../../../../core/models/shipping-rule.model';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LocalizedPricePipe } from '../../../../../core/pipes/localized-price.pipe';

@Component({
  selector: 'app-shipping-rules',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, LocalizedPricePipe],
  templateUrl: './shipping-rules.html',
  styleUrl: './shipping-rules.css'
})
export class ShippingRules implements OnInit {
  rules: ShippingRule[] = [];
  categories: any[] = [];
  isLoading = false;
  isSubmitting = false;
  showModal = false;
  isEditing = false;
  editingId: number | null = null;
  
  ruleForm!: FormGroup;
  successMessage = '';
  errorMessage = '';

  cities: string[] = [
    'Cairo', 'Giza', 'Alexandria', 'Aswan', 'Luxor', 
    'Port Said', 'Suez', 'Mansoura', 'Tanta', 'Ismailia', 'Assiut'
  ];

  constructor(
    private shippingRulesService: ShippingRulesService,
    private categoryService: CategoryService,
    private fb: FormBuilder,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadData();
  }

  initForm(): void {
    this.ruleForm = this.fb.group({
      city: ['', Validators.required],
      categoryId: ['', Validators.required],
      price: ['', [Validators.required, Validators.min(0)]]
    });
  }

  loadData(): void {
    this.isLoading = true;
    this.shippingRulesService.getAll().subscribe({
      next: (res) => {
        this.rules = res;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching rules', err);
        this.isLoading = false;
        this.errorMessage = 'ALERTS.LOAD_ERROR';
      }
    });

    this.categoryService.getAllCategories(1, 100).subscribe({
      next: (res: any) => {
        this.categories = res;
      }
    });
  }

  openAddModal(): void {
    this.isEditing = false;
    this.editingId = null;
    this.ruleForm.reset({ city: '', categoryId: '', price: '' });
    this.clearMessages();
    this.showModal = true;
  }

  openEditModal(rule: ShippingRule): void {
    this.isEditing = true;
    this.editingId = rule.id;
    this.clearMessages();
    this.ruleForm.patchValue({
      city: rule.city,
      categoryId: rule.categoryId,
      price: rule.price
    });
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.clearMessages();
  }

  onSubmit(): void {
    if (this.ruleForm.invalid) {
      this.ruleForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.clearMessages();

    const formValue = this.ruleForm.value;
    const dto: ShippingRuleCreateUpdate = {
      city: formValue.city,
      categoryId: Number(formValue.categoryId),
      price: Number(formValue.price)
    };

    if (this.isEditing && this.editingId) {
      this.shippingRulesService.update(this.editingId, dto).subscribe({
        next: () => {
          this.handleSuccess('ADMIN.RULE_SAVED');
        },
        error: (err) => this.handleError(err)
      });
    } else {
      this.shippingRulesService.create(dto).subscribe({
        next: () => {
          this.handleSuccess('ADMIN.RULE_SAVED');
        },
        error: (err) => this.handleError(err)
      });
    }
  }

  deleteRule(id: number): void {
    if (confirm(this.translate.instant('ALERTS.DELETE_CONFIRM'))) {
      this.shippingRulesService.delete(id).subscribe({
        next: () => {
          this.loadData();
          // Optional: toast for delete
        },
        error: (err) => console.error('Error deleting rule', err)
      });
    }
  }

  private handleSuccess(msgKey: string): void {
    this.successMessage = msgKey;
    this.isSubmitting = false;
    this.loadData();
    setTimeout(() => this.closeModal(), 1200);
  }

  private handleError(err: any): void {
    this.errorMessage = err.error?.message || 'ALERTS.ERROR';
    this.isSubmitting = false;
  }

  private clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }
}
