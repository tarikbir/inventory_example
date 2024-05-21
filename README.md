# Example Inventory System

## Overview

This repository contains a basic inventory system implemented in Unity, focusing on `Inventory` and `Item` classes. It's stripped from another game I was working on. It serves as a proof of concept to demonstrate the ability to manage an inventory. The system emphasizes a clean and maintainable architecture with Separation of Concerns. The UI components (missing here) are designed to subscribe to the core inventory logic (events are also stripped), ensuring a decoupled design.

## Features

- Basic functionality to add, remove, and manage items within the inventory.
- Handles item storage, including capacity checks and item movement.
- Designed so that UI components or other game elements can subscribe to inventory changes, promoting a decoupled and maintainable codebase.

## Design Principles

- **Separation of Concerns**: The inventory logic is separated from the UI, allowing for easier maintenance and scalability. The UI components are subscribers to events triggered by the inventory, ensuring minimal dependencies.
- **Modularity**: The system is built with modularity in mind, allowing for easy extensions and modifications.
- **Serialization**: The inventory and items inside can be serialized (to JSON) to ensure saving and loading without data loss.
