export type LedgerType = 'Credit' | 'Debit';

export interface LedgerEntry {
  id: number;
  date: string;
  type: LedgerType;
  amount: number;
  category: string;
  note?: string;
  createdBy: string;
  runningBalance: number;
}

export interface CreateLedgerEntryRequest {
  date: string;
  type: LedgerType;
  amount: number;
  category: string;
  note?: string;
}

export interface LedgerSummary {
  date: string;
  totalCredit: number;
  totalDebit: number;
  net: number;
  entryCount: number;
}

export const CREDIT_CATEGORIES = ['Sales', 'Cash In', 'Advance Received', 'Refund In', 'Other Income'];
export const DEBIT_CATEGORIES  = ['Supplier Payment', 'Utility Bill', 'Salary', 'Rent', 'Maintenance', 'Petty Cash', 'Other Expense'];
