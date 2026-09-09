/** Builds a minimal ZPL label for Zebra printers: a title line + a Code128 barcode with human-readable value beneath it. */
export function buildBarcodeZpl(title: string, barcodeValue: string): string {
  return [
    '^XA',
    '^PW400',
    '^LL240',
    '^CF0,28',
    `^FO20,20^FD${escapeZpl(title)}^FS`,
    '^BY2,2,80',
    `^FO20,60^BCN,80,Y,N,N^FD${escapeZpl(barcodeValue)}^FS`,
    '^XZ',
  ].join('\n');
}

// ZPL uses ^ and ~ as command prefixes — strip them from user-supplied text so a barcode value can't inject commands.
function escapeZpl(value: string): string {
  return value.replace(/[\^~]/g, '');
}
