export interface RatingStats {
  foodItemId: number;
  averageRating: number;
  ratingCount: number;
}

export interface RecommendedItem {
  foodItemId: number;
  itemName: string;
  categoryName: string;
  price: number;
  image: string | null;
  averageRating: number;
  ratingCount: number;
  orderCount: number;
}

export interface SubmitRatingRequest {
  foodItemId: number;
  rating: number;
  comment?: string;
  sessionId: string;
  tableNumber?: string;
}
