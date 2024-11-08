# Detrav Recipe Calculator

**Detrav Recipe Calculator** is a tool designed to streamline complex crafting calculations by providing a structured, visual, and customizable approach. It guides users through all necessary steps to accurately plan and manage resources across various crafting tiers. Perfect for gamers, modders, and developers working on resource management, Detrav Recipe Calculator supports desktop platforms including **Windows**, **Linux**, and **macOS**.

## Key Features

1. **Recipe Creation and Customization**
   - Add and configure recipes with ease.
   - Adjust details for each recipe, ensuring all requirements align accurately.

2. **Resource Management**
   - Set up and manage resources essential to crafting, enabling accurate resource allocation.

3. **Visual Node-Based Editor**
   - Build and manage a crafting flow through an intuitive, node-based editor.
   - Track detailed statistics for each node, including total resource requirements, resulting outputs, and any excess or surplus resources.

4. **Advanced Crafting Calculations**
   - Use mathematical expressions for tier-based adjustments. For example, define scaling for Tier 1 and Tier 2 resources; simply input expressions like `value * tier` to dynamically calculate resource quantities on the graph.

5. **Color and Icon Customization**
   - Assign custom colors to nodes, resources, and connectors for easier visual distinction.
   - Choose from a library of in-game icons to personalize recipes and resources or add custom ones.

6. **Undo/Redo Support**
   - Full undo and redo functionality to ensure changes can be easily modified or reverted.

7. **Save and Load Projects**
   - Save crafting calculations and load them when needed, making it simple to resume complex projects.

## Future Development

Unfortunately, I don't have time to do support, but if I see a notification from github I will try to help, if you suddenly wrote, and I don't answer, most likely I am swamped with work and I don't have time to check the issue.


## How It Works

The calculator is broken down into four main stages:

1. **Add Recipes**
   - Define the initial recipes for your crafting process. Each recipe can be added with its respective details and configurations.

2. **Verify and Configure Recipes**
   - Review and fine-tune each recipe, ensuring all values, inputs, and outputs are correct and adjusted to your needs.

3. **Configure Resources**
   - Set up the resources required for your recipes, including quantities and other relevant properties.

4. **Build the Crafting Graph**
   - Use the visual editor to create a node-based graph of your crafting workflow. The graph provides a comprehensive view of your resource requirements and surplus, including a summary of total quantities and any excess materials.

## Installation

To install and use Detrav Recipe Calculator, follow these steps:

1. Download the latest release from [Releases](link-to-release).
2. Follow platform-specific instructions for **Windows**, **Linux**, or **macOS**.
3. Launch the application and start building your first crafting project!

## Uninstallation

Delete the folder where the executable file was located, and also delete the folder with the settings in the directory `%APPDATA%/DetravRecipeCalculator`.

## Summary

Detrav Recipe Calculator is designed to simplify and elevate the process of crafting planning, providing a high degree of customization and control. Whether you’re managing resources for a game or organizing complex production workflows, Detrav Recipe Calculator has you covered.


## Icons and Assets

- **Game Icons**: Icons sourced from [game-icons.net](https://game-icons.net) are available under the **Creative Commons Attribution 3.0 License**. This means you are free to use and modify them, as long as you provide appropriate attribution.

- **App Icons**: Icons sourced from [icons8.com](https://icons8.com) are available under the **Icons8 License**. You can use these icons for free in your application, provided that you include a link to Icons8 in your README or provide appropriate credit.

Thank you to these resources for providing high-quality icons that enhance the usability and aesthetics of Detrav Recipe Calculator!

## ToDo



17. add total node
18. switching time (ticks (minecraft), seconds, minutes, hours, days, weeks, months (30 days), years(365 days))	
21. done expression editor
22. add short resource name
23. project templates for new (create)
24. Save render of graph
25. show error if node has deleted resource
26. resource renaming for graph models (after adding save load graph)
27. added warning sing in node view for resource warnings (mb errors?)

## Changelog

Simple changelog, read from top to bottom.

```
Version 0.1
-----------

+ Initial release.

Version 0.2
---------------

* Removed icon pack from the repository.
+ Added prompt to download the icon pack when opening the icon selection window.
+ Added download prompt localization.
+ Added format description to color tooltip: #rgb, #argb, #rrggbb, #aarrggbb.
+ Added ability to paste icon from clipboard.
+ Added color preview for background image, foreground image, and connection color.
+ Added color filter for icons.
+ Added 3px corner radius for icons.
+ Added icon preview to listbox.

Version 0.3
---------------
 
+ Added prototype of node graph editor.
+ Added node preview in the middle column with clickable functionality, allowing future calculation previews.
+ Added icons for tier switching.
+ Added functionality to adjust the number of crafts (machines).
+ Added colors for connectors.
+ Added connection lines with support for resource colors.
+ Added ability to delete connection lines.
+ Added resource icons on nodes.
+ Added basic resource calculation and display.
+ Added graph state serialization.
+ Added ability to copy and paste nodes.
* Modified node pasting mechanics: pasted nodes are now selected by default.
+ Added common editing shortcuts: undo, redo, cut, copy, delete, paste.
* Moved shortcuts to a separate menu, as they were not frequently used in the context menu.
+ Added undo and redo functionality for the graph editor.
+ Added save and load functionality for the graph editor model.
+ Added graph editor prototype.
+ Added localization support to the graph editor.

Current Version
---------------


```

