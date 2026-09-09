export type ReservationStatus = 'Pending' | 'Confirmed' | 'Seated' | 'Cancelled' | 'NoShow';

export interface ReservationDto {
  id: number;
  tableId: number;
  tableNumber: number;
  hall: string;
  guestName: string;
  guestPhone: string;
  partySize: number;
  reservationDateTime: string;
  notes?: string;
  status: ReservationStatus;
  createdDate: string;
}

export interface CreateReservationRequest {
  tableId: number;
  guestName: string;
  guestPhone: string;
  partySize: number;
  reservationDateTime: string;
  notes?: string;
}

export interface UpdateReservationRequest {
  guestName: string;
  guestPhone: string;
  partySize: number;
  reservationDateTime: string;
  notes?: string;
}

export interface UpdateReservationStatusRequest {
  status: ReservationStatus;
}
