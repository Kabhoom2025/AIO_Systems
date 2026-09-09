import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class WhatsAppService {

  sendOrderConfirmation(phone: string, order: {
    orderNumber: string;
    items: { itemName: string; quantity: number; lineTotal: number }[];
    subTotal: number;
    tax: number;
    discount: number;
    grandTotal: number;
    restaurantName?: string;
  }): void {
    const lines: string[] = [];
    lines.push(`🍽️ *Order Confirmed!*`);
    if (order.restaurantName) lines.push(`📍 ${order.restaurantName}`);
    lines.push('');
    lines.push(`📋 *Order:* ${order.orderNumber}`);
    lines.push(`📅 ${new Date().toLocaleString('en-IN', { dateStyle: 'medium', timeStyle: 'short' })}`);
    lines.push('');
    lines.push('*Items Ordered:*');
    order.items.forEach(i => {
      lines.push(`  • ${i.itemName} ×${i.quantity} — ₹${i.lineTotal.toFixed(2)}`);
    });
    lines.push('');
    lines.push(`💰 Subtotal : ₹${order.subTotal.toFixed(2)}`);
    lines.push(`🧾 Tax      : ₹${order.tax.toFixed(2)}`);
    if (order.discount > 0) {
      lines.push(`✂️ Discount : -₹${order.discount.toFixed(2)}`);
    }
    lines.push(`*Total     : ₹${order.grandTotal.toFixed(2)}*`);
    lines.push('');
    lines.push('Thank you for your order! We\'ll get it ready soon. 😊');

    this.open(phone, lines.join('\n'));
  }

  sendOrderCancellation(phone: string, order: {
    orderNumber: string;
    grandTotal: number;
    restaurantName?: string;
  }): void {
    const lines: string[] = [];
    lines.push(`❌ *Order Cancelled*`);
    if (order.restaurantName) lines.push(`📍 ${order.restaurantName}`);
    lines.push('');
    lines.push(`📋 *Order ${order.orderNumber}* has been cancelled.`);
    lines.push(`💰 Amount: ₹${order.grandTotal.toFixed(2)}`);
    lines.push('');
    lines.push('We\'re sorry for the inconvenience. Please visit us again! 🙏');

    this.open(phone, lines.join('\n'));
  }

  private open(phone: string, message: string): void {
    const digits = phone.replace(/\D/g, '');
    const e164 = digits.startsWith('91') && digits.length === 12 ? digits : `91${digits}`;
    const url = `https://wa.me/${e164}?text=${encodeURIComponent(message)}`;
    window.open(url, '_blank', 'noopener');
  }
}
