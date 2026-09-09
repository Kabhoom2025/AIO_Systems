export interface MedicineBatch {
  id: number;
  medicineId: number;
  batchNumber: string;
  expiryDate: string;
  manufacturingDate?: string | null;
  quantityReceived: number;
  currentQuantity: number;
  purchasePrice: number;
  daysToExpiry: number;
  expiryStatus: string;
}

export interface Medicine {
  id: number;
  name: string;
  genericName: string;
  category: string;
  drugSchedule: string;
  packType: string;
  packSize: string;
  unit: string;
  manufacturer: string;
  hsnCode?: string | null;
  mrp: number;
  purchasePrice: number;
  gstPercent: number;
  rackLocation?: string | null;
  reorderLevel: number;
  isActive: boolean;
  totalStock: number;
  batches: MedicineBatch[];
}

export interface ExpiryAlert {
  medicineId: number;
  medicineName: string;
  genericName: string;
  batchId: number;
  batchNumber: string;
  expiryDate: string;
  daysToExpiry: number;
  quantity: number;
  expiryStatus: string;
  rackLocation?: string | null;
}

export interface PrescriptionItem {
  id: number;
  medicineName: string;
  dosage: string;
  duration: string;
  quantity: number;
  isDispensed: boolean;
}

export interface Prescription {
  id: number;
  patientName: string;
  patientPhone?: string | null;
  patientAge?: number | null;
  doctorName: string;
  doctorRegNo?: string | null;
  hospitalName?: string | null;
  prescriptionDate: string;
  imageBase64?: string | null;
  status: string;
  notes?: string | null;
  createdDate: string;
  items: PrescriptionItem[];
}
