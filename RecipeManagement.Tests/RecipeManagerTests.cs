using System;
using System.Collections.Generic;
using RecipeManagement.Core;
using Xunit;

namespace RecipeManagement.Tests;

/// <summary>
/// Tests covering Part A data structures: Dictionary, List, LinkedList, Stack, Queue.
/// </summary>
public sealed class RecipeManagerTests
{
    // ===== Existing example tests =====

    [Fact]
    public void Constructor_BuildsRecipeDictionary()
    {
        var manager = CreateManager();
        Assert.Equal(2, manager.RecipeCount);
        Assert.Equal("Recipe A", manager.FindRecipe(10)?.Title);
    }

    [Fact]
    public void InstructionsAreCompletedInFileOrder()
    {
        var manager = CreateManager();
        Assert.True(manager.StartCooking(10));
        Assert.Equal("First step", manager.PeekNextInstruction());
        Assert.Equal("First step", manager.CompleteNextInstruction());
        Assert.Equal("Second step", manager.PeekNextInstruction());
    }

    [Fact]
    public void RemovedRecipesAreRestoredLastInFirstOut()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.RemoveRecipeFromCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(20);
        Assert.Equal(20, manager.PeekLastRemovedRecipe());
        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 20 }, manager.GetCookingPlan());
    }

    // ===== Constructor validation =====

    [Fact]
    public void Constructor_NullRecipes_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RecipeManager(null!));
    }

    [Fact]
    public void Constructor_DuplicateId_ThrowsArgumentException()
    {
        var recipes = new[]
        {
            new Recipe { Id = 1, Title = "A" },
            new Recipe { Id = 1, Title = "B" }
        };
        Assert.Throws<ArgumentException>(() => new RecipeManager(recipes));
    }

    [Fact]
    public void Constructor_NonPositiveId_ThrowsArgumentException()
    {
        var recipes = new[] { new Recipe { Id = 0, Title = "Bad" } };
        Assert.Throws<ArgumentException>(() => new RecipeManager(recipes));
    }

    [Fact]
    public void Constructor_BlankTitle_ThrowsArgumentException()
    {
        var recipes = new[] { new Recipe { Id = 1, Title = "   " } };
        Assert.Throws<ArgumentException>(() => new RecipeManager(recipes));
    }

    // ===== Dictionary: Add / Find / Remove =====

    [Fact]
    public void AddRecipe_DuplicateId_ReturnsFalse()
    {
        var manager = CreateManager();
        var result = manager.AddRecipe(new Recipe { Id = 10, Title = "Duplicate" });
        Assert.False(result);
        Assert.Equal(2, manager.RecipeCount);
    }

    [Fact]
    public void AddRecipe_NonPositiveId_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.AddRecipe(new Recipe { Id = -5, Title = "Bad" }));
        Assert.False(manager.AddRecipe(new Recipe { Id = 0, Title = "Bad" }));
    }

    [Fact]
    public void AddRecipe_BlankTitle_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.AddRecipe(new Recipe { Id = 99, Title = "  " }));
    }

    [Fact]
    public void AddRecipe_ValidRecipe_ReturnsTrue()
    {
        var manager = CreateManager();
        Assert.True(manager.AddRecipe(new Recipe { Id = 30, Title = "New" }));
        Assert.Equal(3, manager.RecipeCount);
        Assert.Equal("New", manager.FindRecipe(30)?.Title);
    }

    [Fact]
    public void FindRecipe_MissingId_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(manager.FindRecipe(999));
    }

    [Fact]
    public void RemoveRecipe_MissingId_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.RemoveRecipe(999));
    }

    [Fact]
    public void RemoveRecipe_NotInPlan_ReturnsTrue()
    {
        var manager = CreateManager();
        Assert.True(manager.RemoveRecipe(20));
        Assert.Null(manager.FindRecipe(20));
        Assert.Equal(1, manager.RecipeCount);
    }

    [Fact]
    public void RemoveRecipe_InCookingPlan_ReturnsFalse()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        Assert.False(manager.RemoveRecipe(10));
        Assert.NotNull(manager.FindRecipe(10));
    }

    // ===== List: Shopping list =====

    [Fact]
    public void AddIngredients_CopiesIngredientsInOrder()
    {
        var manager = CreateManager();
        int added = manager.AddIngredientsToShoppingList(10);
        Assert.Equal(1, added);
        Assert.Equal(new[] { "1 apple" }, manager.GetShoppingList());
    }

    [Fact]
    public void AddIngredients_MissingRecipe_ReturnsZero()
    {
        var manager = CreateManager();
        Assert.Equal(0, manager.AddIngredientsToShoppingList(999));
        Assert.Empty(manager.GetShoppingList());
    }

    [Fact]
    public void ClearShoppingList_RemovesAllItems()
    {
        var manager = CreateManager();
        manager.AddIngredientsToShoppingList(10);
        manager.ClearShoppingList();
        Assert.Empty(manager.GetShoppingList());
        Assert.Equal(0, manager.ShoppingItemCount);
    }

    // ===== LinkedList: Cooking plan =====

    [Fact]
    public void AddToCookingPlan_DuplicateRecipe_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.True(manager.AddRecipeToCookingPlan(10));
        Assert.False(manager.AddRecipeToCookingPlan(10));
        Assert.Equal(1, manager.CookingPlanCount);
    }

    [Fact]
    public void AddToCookingPlan_MissingRecipe_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.AddRecipeToCookingPlan(999));
    }

    [Fact]
    public void RemoveFromCookingPlan_NotPlanned_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.RemoveRecipeFromCookingPlan(999));
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void CookingPlan_PreservesInsertionOrder()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(20);
        manager.AddRecipeToCookingPlan(10);
        Assert.Equal(new[] { 20, 10 }, manager.GetCookingPlan());
    }

    // ===== Stack: Removed recipe history =====

    [Fact]
    public void PeekLastRemoved_EmptyStack_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(manager.PeekLastRemovedRecipe());
    }

    [Fact]
    public void RestoreLastRemoved_EmptyStack_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.RestoreLastRemovedRecipe());
    }

    [Fact]
    public void RestoreLastRemoved_RecipeAlreadyInPlan_ReturnsFalse()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);
        // Plan is now empty; add 10 back manually, then try to restore from stack
        manager.AddRecipeToCookingPlan(10);
        // Stack still has 10, but 10 is already in the plan
        Assert.False(manager.RestoreLastRemovedRecipe());
    }

    // ===== Queue: Cooking instructions =====

    [Fact]
    public void PeekNextInstruction_EmptyQueue_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(manager.PeekNextInstruction());
    }

    [Fact]
    public void CompleteNextInstruction_EmptyQueue_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(manager.CompleteNextInstruction());
    }

    [Fact]
    public void StartCooking_RecipeNoInstructions_ReturnsFalse()
    {
        var manager = CreateManager();
        // Recipe 20 has no instructions
        Assert.False(manager.StartCooking(20));
    }

    [Fact]
    public void StartCooking_MissingRecipe_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.StartCooking(999));
    }

    [Fact]
    public void StartCooking_ClearsPreviousQueue()
    {
        var manager = CreateManager();
        manager.StartCooking(10);
        Assert.Equal(2, manager.PendingInstructionCount);

        // Add a third recipe with its own instructions and start cooking it
        manager.AddRecipe(new Recipe
        {
            Id = 30,
            Title = "Recipe C",
            Instructions = new() { "New step" }
        });
        Assert.True(manager.StartCooking(30));

        // Old instructions must be gone; only the new recipe's step remains
        Assert.Equal(1, manager.PendingInstructionCount);
        Assert.Equal("New step", manager.PeekNextInstruction());
    }

    // ===== Component interaction: full workflow =====

    [Fact]
    public void FullWorkflow_CatalogueShoppingPlanCooking()
    {
        var manager = CreateManager();

        // Add ingredients to shopping list
        Assert.Equal(1, manager.AddIngredientsToShoppingList(10));
        Assert.Equal(1, manager.ShoppingItemCount);

        // Add to cooking plan
        Assert.True(manager.AddRecipeToCookingPlan(10));
        Assert.True(manager.AddRecipeToCookingPlan(20));
        Assert.Equal(new[] { 10, 20 }, manager.GetCookingPlan());

        // Remove from plan → pushes to stack
        Assert.True(manager.RemoveRecipeFromCookingPlan(10));
        Assert.Equal(1, manager.RemovedRecipeCount);
        Assert.Equal(10, manager.PeekLastRemovedRecipe());

        // Restore → back at end of plan
        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 20, 10 }, manager.GetCookingPlan());

        // Start cooking and process instructions
        Assert.True(manager.StartCooking(10));
        Assert.Equal("First step", manager.CompleteNextInstruction());
        Assert.Equal("Second step", manager.CompleteNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());
    }

    // ===== Helpers =====

    private static RecipeManager CreateManager()
    {
        return new RecipeManager(new[]
        {
            new Recipe
            {
                Id = 10,
                Title = "Recipe A",
                Ingredients = new() { "1 apple" },
                Instructions = new() { "First step", "Second step" }
            },
            new Recipe
            {
                Id = 20,
                Title = "Recipe B"
            }
        });
    }
}
