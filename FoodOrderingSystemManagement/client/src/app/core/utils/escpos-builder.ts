import { Order } from '../models/order.model';
import { KdsItem } from '../../features/kds/kds.component';
import { Settings } from '../models/settings.model';

// Standard ESC/POS control codes for 42-column (58/80mm) thermal printers.
const ESC = '\x1B';
const GS  = '\x1D';
const INIT        = `${ESC}@`;
const ALIGN_LEFT   = `${ESC}a0`;
const ALIGN_CENTER = `${ESC}a1`;
const BOLD_ON      = `${ESC}E1`;
const BOLD_OFF     = `${ESC}E0`;
const DOUBLE_ON    = `${GS}!17`;   // double width + height
const DOUBLE_OFF   = `${GS}!0`;
const CUT          = `${GS}V1`;
const LF           = '\n';

const COLS = 42;

function padCols(cells: string[], widths: number[]): string {
  return cells
    .map((cell, i) => {
      const w = widths[i];
      return i === widths.length - 1
        ? cell.slice(0, w).padStart(w)
        : cell.slice(0, w).padEnd(w);
    })
    .join('');
}

function line(char = '-'): string {
  return char.repeat(COLS) + LF;
}

/** Builds a full ESC/POS receipt (header, items, totals, footer) ready to send raw to a thermal printer. */
export function buildReceiptEscPos(order: Order, settings: Settings | null | undefined): string {
  let out = INIT + ALIGN_CENTER;

  out += BOLD_ON + DOUBLE_ON + (settings?.restaurantName || 'FoodOrder POS') + DOUBLE_OFF + BOLD_OFF + LF;
  if (settings?.address)   out += settings.address + LF;
  if (settings?.phone)     out += `Ph: ${settings.phone}` + LF;
  if (settings?.gstNumber) out += `GST: ${settings.gstNumber}` + LF;
  out += LF;

  out += ALIGN_LEFT;
  out += line();
  out += `Order #: ${order.orderNumber}` + LF;
  out += `Cashier: ${order.cashierName}` + LF;
  out += `Date: ${new Date(order.orderDate).toLocaleString()}` + LF;
  out += line();

  out += BOLD_ON + padCols(['Item', 'Qty', 'Price', 'Total'], [18, 6, 8, 10]) + BOLD_OFF + LF;
  out += line();
  for (const item of order.items) {
    out += padCols(
      [item.itemName, String(item.quantity), item.unitPrice.toFixed(2), item.lineTotal.toFixed(2)],
      [18, 6, 8, 10],
    ) + LF;
  }
  out += line();

  out += padCols(['Subtotal', order.subTotal.toFixed(2)], [32, 10]) + LF;
  out += padCols(['Tax', order.tax.toFixed(2)], [32, 10]) + LF;
  if (order.discount > 0) {
    out += padCols(['Discount', `-${order.discount.toFixed(2)}`], [32, 10]) + LF;
  }
  out += line();
  out += BOLD_ON + padCols(['Grand Total', order.grandTotal.toFixed(2)], [32, 10]) + BOLD_OFF + LF;
  out += line();

  if (order.customerName) {
    out += `Customer: ${order.customerName} (${order.customerTier ?? ''})` + LF;
    if (order.pointsRedeemed) out += padCols(['Points Redeemed', `-${order.pointsRedeemed}`], [32, 10]) + LF;
    if (order.pointsEarned)   out += padCols(['Points Earned', `+${order.pointsEarned}`], [32, 10]) + LF;
    if (order.newPointsBalance != null) out += padCols(['New Balance', String(order.newPointsBalance)], [32, 10]) + LF;
    out += line();
  }

  out += ALIGN_CENTER + LF + 'Thank you for dining with us!' + LF + LF + LF;
  out += CUT;
  return out;
}

/** Builds a simple kitchen-order-ticket (no prices) for the KDS "print slip" action. */
export function buildKotEscPos(orderNumber: string, tableNumber: number | null, items: KdsItem[]): string {
  let out = INIT + ALIGN_CENTER;
  out += BOLD_ON + DOUBLE_ON + 'KITCHEN ORDER TICKET' + DOUBLE_OFF + BOLD_OFF + LF + LF;
  out += ALIGN_LEFT;
  out += `Order #: ${orderNumber}` + LF;
  out += `Table: ${tableNumber ?? 'Takeaway'}` + LF;
  out += `Time: ${new Date().toLocaleTimeString()}` + LF;
  out += line();
  for (const item of items) {
    out += BOLD_ON + `${item.qty} x ${item.name}` + BOLD_OFF + LF;
    if (item.addOnNotes) out += `   note: ${item.addOnNotes}` + LF;
  }
  out += line();
  out += LF + LF;
  out += CUT;
  return out;
}
