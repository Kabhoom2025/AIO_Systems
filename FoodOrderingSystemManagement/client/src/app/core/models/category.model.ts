export interface Category {
  id: number;
  categoryName: string;
  description: string;
  image: string;
  displayOrder: number;
  isActive: boolean;
  createdDate: string;
}

export interface CreateCategoryRequest {
  categoryName: string;
  description?: string;
  image?: string;
  displayOrder?: number;
}

export interface UpdateCategoryRequest extends CreateCategoryRequest {
  isActive: boolean;
}
