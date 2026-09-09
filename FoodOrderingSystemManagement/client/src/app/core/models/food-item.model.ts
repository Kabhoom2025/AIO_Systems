export interface FoodItem {
  id: number;
  categoryId: number;
  categoryName: string;
  itemName: string;
  description: string;
  image: string;
  price: number;
  availableQuantity: number;
  isAvailable: boolean;
  barcode?: string;
  createdDate: string;
}

export interface CreateFoodItemRequest {
  categoryId: number;
  itemName: string;
  description?: string;
  image?: string;
  price: number;
  availableQuantity: number;
  barcode?: string;
}

export interface UpdateFoodItemRequest extends CreateFoodItemRequest {
  isAvailable: boolean;
}
