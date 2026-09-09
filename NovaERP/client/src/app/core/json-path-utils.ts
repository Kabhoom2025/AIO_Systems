/// Recursively flattens a parsed JSON value into a list of leaf paths using the same dot/
/// bracket convention the backend JsonPathWalker understands: arrays collapse to a single
/// "foo[]" entry (not one per index) since every element of an array is expected to have the
/// same shape (a rate-shop response's "rates" array, a request's "packages" array, etc).
export interface DetectedField {
  path: string;
  example: string;
}

export function flattenJsonPaths(value: unknown, prefix = ''): DetectedField[] {
  if (value === null || value === undefined) {
    return prefix ? [{ path: prefix, example: 'null' }] : [];
  }

  if (Array.isArray(value)) {
    if (value.length === 0) return [];
    const arrayPath = `${prefix}[]`;
    return flattenJsonPaths(value[0], arrayPath);
  }

  if (typeof value === 'object') {
    return Object.entries(value as Record<string, unknown>).flatMap(([key, v]) =>
      flattenJsonPaths(v, prefix ? `${prefix}.${key}` : key)
    );
  }

  return [{ path: prefix, example: String(value) }];
}

export function tryFlattenJsonPaths(rawJson: string): DetectedField[] {
  try {
    return flattenJsonPaths(JSON.parse(rawJson));
  } catch {
    return [];
  }
}

/// XML analog of flattenJsonPaths, matching XmlPathWalker's addressing convention: paths are
/// relative to the parsed document's root element's own children (the root tag itself isn't
/// part of the path, same as a JSON root object's properties being addressed without naming
/// the object). Repeated same-tag siblings under one parent collapse to a single "tag[]" entry.
export function tryFlattenXmlPaths(rawXml: string): DetectedField[] {
  try {
    const doc = new DOMParser().parseFromString(rawXml, 'application/xml');
    if (doc.querySelector('parsererror') || !doc.documentElement) return [];
    return flattenXmlElement(doc.documentElement, '');
  } catch {
    return [];
  }
}

function flattenXmlElement(element: Element, prefix: string): DetectedField[] {
  const childElements = Array.from(element.children);
  if (childElements.length === 0) {
    const text = (element.textContent ?? '').trim();
    return prefix ? [{ path: prefix, example: text || '(empty)' }] : [];
  }

  const seenTags = new Set<string>();
  const results: DetectedField[] = [];
  for (const child of childElements) {
    const tag = child.tagName;
    if (seenTags.has(tag)) continue;
    seenTags.add(tag);

    const siblingCount = childElements.filter(c => c.tagName === tag).length;
    const childPrefix = siblingCount > 1
      ? (prefix ? `${prefix}.${tag}[]` : `${tag}[]`)
      : (prefix ? `${prefix}.${tag}` : tag);
    results.push(...flattenXmlElement(child, childPrefix));
  }
  return results;
}

/// Flattens an application/x-www-form-urlencoded sample (either a raw "key=val&key2=val2"
/// query string, or one key=value pair per line — both are common ways to paste one of these
/// legacy carrier API samples) into a field list, collapsing numeric per-package indices like
/// "wweight[1]", "wweight[2]" into a single "wweight[]" entry (matching the "[]" repeat-per-
/// package convention the backend's form-field builder uses).
export function tryFlattenFormPaths(rawForm: string): DetectedField[] {
  try {
    const pairs = rawForm
      .split(/[&\n]/)
      .map(p => p.trim())
      .filter(p => p.length > 0 && p.includes('='));

    const seen = new Map<string, string>();
    for (const pair of pairs) {
      const eq = pair.indexOf('=');
      const rawKey = decodeURIComponent(pair.slice(0, eq).replace(/\+/g, ' '));
      const rawValue = decodeURIComponent(pair.slice(eq + 1).replace(/\+/g, ' '));
      const groupedKey = rawKey.replace(/\[\d+\]/g, '[]');
      if (!seen.has(groupedKey)) seen.set(groupedKey, rawValue);
    }
    return Array.from(seen.entries()).map(([path, example]) => ({ path, example }));
  } catch {
    return [];
  }
}
