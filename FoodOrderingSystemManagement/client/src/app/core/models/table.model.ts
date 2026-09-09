export type HallType = 'AC' | 'Non-AC';
export const HALL_OPTIONS: HallType[] = ['AC', 'Non-AC'];

export interface Table {
  id: number;
  tableNumber: number;
  capacity: number;
  hall: HallType;
  isActive: boolean;
  isOccupied: boolean;
  billPending?: boolean;
}

export interface CreateTableRequest {
  tableNumber: number;
  capacity: number;
  hall: HallType;
}

export interface UpdateTableRequest {
  tableNumber: number;
  capacity: number;
  hall: HallType;
  isActive: boolean;
}
