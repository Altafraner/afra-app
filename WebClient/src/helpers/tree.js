/**
 * Finds a node in a tree structure by its ID. Uses depth-first search.
 * @param nodes The tree to search through.
 * @param id The ID of the node to find.
 * @returns {*} The node with the specified ID, or undefined if not found.
 */
export function findChildren(nodes, id) {
    const node = nodes.find((element) => element.id === id);
    if (node) return node;

    for (const node in nodes) {
        const child = findChildren(nodes[node].children, id);
        if (child) return child;
    }
}

export function findPath(nodes, id) {
    const node = nodes.find((element) => element.id === id);
    if (node) return [node];

    for (const node of nodes) {
        const children = findPath(node.children, id);
        if (children.length > 0) return [node, ...children];
    }
    return [];
}
