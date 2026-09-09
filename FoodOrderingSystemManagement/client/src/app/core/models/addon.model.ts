export interface AddOn {
  id: number;
  name: string;
  price: number;
  category?: string;
  isAvailable?: boolean;
}

export interface ItemAddOn {
  itemId: number;
  addOns: AddOn[];
}

export interface CartItemWithAddOns {
  itemId: number;
  itemName: string;
  quantity: number;
  basePrice: number;
  selectedAddOns: AddOn[];
  totalPrice: number;
}
