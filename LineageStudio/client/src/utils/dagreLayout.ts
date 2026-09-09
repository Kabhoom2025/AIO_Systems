import dagre from 'dagre'
import type { Edge, Node } from '@xyflow/react'

/** Auto-layouts a React Flow graph with dagre (left-to-right pipeline layout), per the spec's
 * "use ELK.js or Dagre for graph layout" requirement - mutates nothing, returns new node
 * positions. Nodes keep their own width/height if set on `measured`/`style`, else a default. */
export function layoutWithDagre<N extends Node>(
  nodes: N[],
  edges: Edge[],
  options?: { direction?: 'LR' | 'TB'; nodeWidth?: number; nodeHeight?: number; rankSep?: number; nodeSep?: number },
): N[] {
  const { direction = 'LR', nodeWidth = 190, nodeHeight = 56, rankSep = 90, nodeSep = 40 } = options ?? {}

  const graph = new dagre.graphlib.Graph()
  graph.setDefaultEdgeLabel(() => ({}))
  graph.setGraph({ rankdir: direction, ranksep: rankSep, nodesep: nodeSep })

  for (const node of nodes) {
    graph.setNode(node.id, { width: nodeWidth, height: nodeHeight })
  }
  for (const edge of edges) {
    if (graph.hasNode(edge.source) && graph.hasNode(edge.target)) {
      graph.setEdge(edge.source, edge.target)
    }
  }

  dagre.layout(graph)

  return nodes.map((node) => {
    const position = graph.node(node.id)
    if (!position) return node
    return {
      ...node,
      position: { x: position.x - nodeWidth / 2, y: position.y - nodeHeight / 2 },
    }
  })
}
