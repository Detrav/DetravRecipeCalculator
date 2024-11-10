# Graph/Visualization

Documentation on graph, nodes, and visualization.

## Nodes

The following nodes are available:

### Comment

Allows you to add comments to the graph. All nodes within the comment area move together with it. To prevent this, hold the `Shift` key.

### Result Table

This node is designed for maintaining a summary table. It’s recommended to use only one such node per tree. You can add multiple result tables on the same graph, as long as the trees don’t intersect.

If needed, it’s better to duplicate the tree and use the duplicate for a second result table.

By default, the result table requests resources exactly in the amount that the recipe produces. For finer adjustments, use the "configure" button in the upper-right corner of the node.

### Divider

An auxiliary node that helps with arranging the tree layout more visually.

### Recipes

These are the primary nodes that are added according to the configured recipes.

## Controls

- `Ctrl+C` — copy nodes.
- `Ctrl+V` — paste nodes.
- `Ctrl+X` — cut nodes.
- `Ctrl+Z` — undo an action.
- `Ctrl+Shift+Z` — redo an action.
- `Delete` — delete nodes.