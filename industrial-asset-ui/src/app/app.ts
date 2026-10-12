
import {
  Component,
  afterNextRender,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { AssetService, ApiAsset } from './asset.service';

type AssetStatus =
  | 'Operational'
  | 'Maintenance'
  | 'Out of Service';

interface Asset {
  databaseId: number;
  id: string;
  equipment: string;
  area: string;
  lastMaintenance: string;
  status: AssetStatus;
}

@Component({
  selector: 'app-root',
  imports: [FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private assetService = inject(AssetService);
  private cdr = inject(ChangeDetectorRef);

  searchText = '';
  selectedStatus = 'All';

  showAssetForm = false;
  formError = '';
  loadError = '';
  loadingAssets = true;
  savingAsset = false;
  editingAssetId: number | null = null;

  newAsset = {
    equipment: '',
    area: '',
    lastMaintenance: '',
    status: 'Operational' as AssetStatus
  };

  assets: Asset[] = [];

  constructor() {
    afterNextRender(() => {
      this.loadAssets();
    });
  }

  loadAssets(): void {
    this.loadingAssets = true;
    this.loadError = '';

    this.assetService.getAssets().subscribe({
      next: (apiAssets) => {
        this.assets = apiAssets.map(asset =>
          this.mapApiAsset(asset)
        );

        this.loadingAssets = false;
        this.cdr.detectChanges();

        console.log('Assets loaded:', this.assets);
      },
      error: (error) => {
        console.error('Failed to load assets:', error);

        this.loadError =
          'Unable to load assets. Check that the .NET API is running at http://localhost:5018.';

        this.loadingAssets = false;
        this.cdr.detectChanges();
      }
    });
  }

  private mapApiAsset(asset: ApiAsset): Asset {
    return {
      databaseId: asset.id,
      id: asset.assetCode,
      equipment: asset.equipment,
      area: asset.area,
      lastMaintenance: this.formatDate(
        asset.lastMaintenance
      ),
      status: asset.status
    };
  }

  private formatDate(value: string): string {
    if (!value) {
      return '';
    }

    const date = new Date(`${value.slice(0, 10)}T00:00:00`);

    return date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    });
  }

  get filteredAssets(): Asset[] {
    const query = this.searchText.trim().toLowerCase();

    return this.assets.filter(asset => {
      const matchesSearch =
        asset.id.toLowerCase().includes(query) ||
        asset.equipment.toLowerCase().includes(query) ||
        asset.area.toLowerCase().includes(query);

      const matchesStatus =
        this.selectedStatus === 'All' ||
        asset.status === this.selectedStatus;

      return matchesSearch && matchesStatus;
    });
  }

  get totalAssets(): number {
    return this.assets.length;
  }

  get operationalCount(): number {
    return this.assets.filter(
      asset => asset.status === 'Operational'
    ).length;
  }

  get maintenanceCount(): number {
    return this.assets.filter(
      asset => asset.status === 'Maintenance'
    ).length;
  }

  get outOfServiceCount(): number {
    return this.assets.filter(
      asset => asset.status === 'Out of Service'
    ).length;
  }

  openAssetForm(): void {
    this.formError = '';
    this.editingAssetId = null;
    this.resetForm();
    this.showAssetForm = true;
  }

  cancelAssetForm(): void {
    this.showAssetForm = false;
    this.formError = '';
    this.editingAssetId = null;
    this.savingAsset = false;
    this.resetForm();
  }

  editAsset(asset: Asset): void {
    this.formError = '';
    this.editingAssetId = asset.databaseId;

    const dateParts = asset.lastMaintenance.split(' ');
    const date = new Date(
      `${dateParts[2]}-${dateParts[1]}-${dateParts[0]}`
    );

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    this.newAsset = {
      equipment: asset.equipment,
      area: asset.area,
      lastMaintenance: `${year}-${month}-${day}`,
      status: asset.status
    };

    this.showAssetForm = true;
  }

  saveAsset(): void {
    if (this.savingAsset) {
      return;
    }

    if (this.editingAssetId !== null) {
      this.updateAsset();
    } else {
      this.addAsset();
    }
  }

  private updateAsset(): void {
    const equipment = this.newAsset.equipment.trim();
    const area = this.newAsset.area.trim();

    if (!equipment || !area || !this.newAsset.lastMaintenance) {
      this.formError = 'Please complete all required fields.';
      return;
    }

    const existingAsset = this.assets.find(
      asset => asset.databaseId === this.editingAssetId
    );

    if (!existingAsset) {
      this.formError = 'Asset not found. Please reload the dashboard.';
      return;
    }

    const duplicate = this.assets.some(
      asset =>
        asset.databaseId !== this.editingAssetId &&
        asset.equipment.toLowerCase() === equipment.toLowerCase()
    );

    if (duplicate) {
      this.formError =
        'An asset with this equipment name already exists.';
      return;
    }

    this.savingAsset = true;
    this.formError = '';

    this.assetService.updateAsset(this.editingAssetId!, {
      assetCode: existingAsset.id,
      equipment,
      area,
      lastMaintenance: this.newAsset.lastMaintenance,
      status: this.newAsset.status
    }).pipe(
      finalize(() => {
        this.savingAsset = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: () => {
        this.editingAssetId = null;
        this.showAssetForm = false;
        this.resetForm();
        this.loadAssets();
      },
      error: (error) => {
        console.error('Failed to update asset:', error);
        this.formError =
          'Unable to update asset. Please try again.';
      }
    });
  }

  deleteAsset(asset: Asset): void {
    const confirmed = confirm(
      `Delete ${asset.id} - ${asset.equipment}? This cannot be undone.`
    );

    if (!confirmed) {
      return;
    }

    this.assetService.deleteAsset(asset.databaseId).subscribe({
      next: () => {
        this.assets = this.assets.filter(
          item => item.databaseId !== asset.databaseId
        );

        if (this.editingAssetId === asset.databaseId) {
          this.editingAssetId = null;
          this.showAssetForm = false;
          this.resetForm();
        }

        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Failed to delete asset:', error);
        alert('Unable to delete asset. Please try again.');
      }
    });
  }

  addAsset(): void {
    if (this.savingAsset) {
      return;
    }

    const equipment = this.newAsset.equipment.trim();
    const area = this.newAsset.area.trim();

    if (!equipment || !area || !this.newAsset.lastMaintenance) {
      this.formError = 'Please complete all required fields.';
      return;
    }

    const duplicate = this.assets.some(
      asset =>
        asset.equipment.toLowerCase() === equipment.toLowerCase()
    );

    if (duplicate) {
      this.formError =
        'An asset with this equipment name already exists.';
      return;
    }

    const nextNumber =
      Math.max(
        0,
        ...this.assets.map(asset => {
          const match = asset.id.match(/^AST-(\d+)$/);
          return match ? Number(match[1]) : 0;
        })
      ) + 1;

    const assetCode =
      `AST-${String(nextNumber).padStart(3, '0')}`;

    this.formError = '';
    this.savingAsset = true;

    this.assetService.createAsset({
      assetCode,
      equipment,
      area,
      lastMaintenance: this.newAsset.lastMaintenance,
      status: this.newAsset.status
    }).pipe(
      finalize(() => {
        this.savingAsset = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (createdAsset) => {
        this.assets = [
          ...this.assets,
          this.mapApiAsset(createdAsset)
        ];

        this.searchText = '';
        this.selectedStatus = 'All';
        this.showAssetForm = false;
        this.formError = '';
        this.editingAssetId = null;
        this.resetForm();

        console.log('Asset saved successfully:', createdAsset);
      },
      error: (error) => {
        console.error('Failed to add asset:', error);

        this.formError =
          error.status === 409
            ? 'This asset code already exists. Reload the dashboard and try again.'
            : 'Unable to save the asset. Check the API and try again.';
      }
    });
  }

  private resetForm(): void {
    this.newAsset = {
      equipment: '',
      area: '',
      lastMaintenance: '',
      status: 'Operational'
    };
  }
}
