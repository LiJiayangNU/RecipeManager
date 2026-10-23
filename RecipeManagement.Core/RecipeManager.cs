using System;
using System.Collections.Generic;
using System.Linq;

namespace RecipeManagement.Core;

/// <summary>
/// Implements the recipe management system using five C# collections:
/// Dictionary<int, Recipe>, List<string>, LinkedList<int>, Stack<int>, Queue<string>.
/// </summary>
public sealed class RecipeManager : IRecipeManager
{
    // Recipe catalogue: primary lookup by recipe ID
    private readonly Dictionary<int, Recipe> _recipes = new();

    // Shopping list: ingredient strings copied from selected recipes
    private readonly List<string> _shoppingList = new();

    // Cooking plan: ordered list of recipe IDs the user intends to cook
    private readonly LinkedList<int> _cookingPlan = new();

    // Removed-recipe history: LIFO stack of recipe IDs removed from the cooking plan
    private readonly Stack<int> _removedRecipes = new();

    // Instruction queue: FIFO queue of cooking steps for the active recipe
    private readonly Queue<string> _instructionQueue = new();

    public RecipeManager(IEnumerable<Recipe> recipes)
    {
        if (recipes is null)
            throw new ArgumentNullException(nameof(recipes));

        foreach (var recipe in recipes)
        {
            if (recipe.Id <= 0)
                throw new ArgumentException(
                    $"Recipe ID must be a positive integer, got {recipe.Id}.", nameof(recipes));

            if (string.IsNullOrWhiteSpace(recipe.Title))
                throw new ArgumentException(
                    $"Recipe with ID {recipe.Id} has a blank title.", nameof(recipes));

            if (_recipes.ContainsKey(recipe.Id))
                throw new ArgumentException(
                    $"Duplicate recipe ID: {recipe.Id}.", nameof(recipes));

            _recipes[recipe.Id] = recipe;
        }
    }

    public int RecipeCount => _recipes.Count;
    public int ShoppingItemCount => _shoppingList.Count;
    public int CookingPlanCount => _cookingPlan.Count;
    public int PendingInstructionCount => _instructionQueue.Count;
    public int RemovedRecipeCount => _removedRecipes.Count;

    // ----- Recipe catalogue (Dictionary) -----

    public bool AddRecipe(Recipe recipe)
    {
        if (recipe is null)
            throw new ArgumentNullException(nameof(recipe));

        if (recipe.Id <= 0) return false;
        if (string.IsNullOrWhiteSpace(recipe.Title)) return false;
        if (_recipes.ContainsKey(recipe.Id)) return false;

        _recipes[recipe.Id] = recipe;
        return true;
    }

    public Recipe? FindRecipe(int recipeId)
    {
        return _recipes.TryGetValue(recipeId, out var recipe) ? recipe : null;
    }

    public bool RemoveRecipe(int recipeId)
    {
        if (!_recipes.ContainsKey(recipeId))
            return false;

        // Cannot remove a recipe that is currently in the cooking plan
        if (_cookingPlan.Contains(recipeId))
            return false;

        _recipes.Remove(recipeId);
        return true;
    }

    // ----- Shopping list (List<string>) -----

    public int AddIngredientsToShoppingList(int recipeId)
    {
        var recipe = FindRecipe(recipeId);
        if (recipe is null)
            return 0;

        foreach (var ingredient in recipe.Ingredients)
        {
            _shoppingList.Add(ingredient);
        }

        return recipe.Ingredients.Count;
    }

    public IReadOnlyList<string> GetShoppingList()
    {
        return _shoppingList.AsReadOnly();
    }

    public void ClearShoppingList()
    {
        _shoppingList.Clear();
    }

    // ----- Cooking plan (LinkedList<int>) -----

    public bool AddRecipeToCookingPlan(int recipeId)
    {
        if (!_recipes.ContainsKey(recipeId))
            return false;

        // A recipe may appear at most once in the plan
        if (_cookingPlan.Contains(recipeId))
            return false;

        _cookingPlan.AddLast(recipeId);
        return true;
    }

    public bool RemoveRecipeFromCookingPlan(int recipeId)
    {
        if (!_cookingPlan.Contains(recipeId))
            return false;

        _cookingPlan.Remove(recipeId);
        _removedRecipes.Push(recipeId);
        return true;
    }

    // ----- Removed-recipe history (Stack<int>) -----

    public bool RestoreLastRemovedRecipe()
    {
        if (_removedRecipes.Count == 0)
            return false;

        int recipeId = _removedRecipes.Pop();

        // Only restore if the recipe still exists and is not already planned
        if (!_recipes.ContainsKey(recipeId))
            return false;
        if (_cookingPlan.Contains(recipeId))
            return false;

        _cookingPlan.AddLast(recipeId);
        return true;
    }

    public int? PeekLastRemovedRecipe()
    {
        return _removedRecipes.Count > 0
            ? _removedRecipes.Peek()
            : null;
    }

    public IReadOnlyList<int> GetCookingPlan()
    {
        return _cookingPlan.ToList();
    }

    // ----- Cooking instructions (Queue<string>) -----

    public bool StartCooking(int recipeId)
    {
        var recipe = FindRecipe(recipeId);
        if (recipe is null || recipe.Instructions.Count == 0)
            return false;

        // Clear any previous instruction queue
        _instructionQueue.Clear();

        foreach (var instruction in recipe.Instructions)
        {
            _instructionQueue.Enqueue(instruction);
        }

        return true;
    }

    public string? PeekNextInstruction()
    {
        return _instructionQueue.Count > 0
            ? _instructionQueue.Peek()
            : null;
    }

    public string? CompleteNextInstruction()
    {
        return _instructionQueue.Count > 0
            ? _instructionQueue.Dequeue()
            : null;
    }

    // ----- Part B (not yet implemented) -----

    public IReadOnlyList<Recipe> SearchByTitle(string searchText) =>
        throw new NotImplementedException("Part B: implement SearchByTitle.");

    public IReadOnlyList<Recipe> SearchByIngredient(string searchText) =>
        throw new NotImplementedException("Part B: implement SearchByIngredient.");

    public IReadOnlyList<Recipe> GetHighestProteinRecipes(int count) =>
        throw new NotImplementedException("Part B: implement GetHighestProteinRecipes.");

    public bool AddSavedRecipe(int recipeId) =>
        throw new NotImplementedException("Part B: implement AddSavedRecipe.");

    public bool RemoveSavedRecipe(int recipeId) =>
        throw new NotImplementedException("Part B: implement RemoveSavedRecipe.");

    public bool IsRecipeSaved(int recipeId) =>
        throw new NotImplementedException("Part B: implement IsRecipeSaved.");

    public IReadOnlyList<int> GetSavedRecipes() =>
        throw new NotImplementedException("Part B: implement GetSavedRecipes.");
}
