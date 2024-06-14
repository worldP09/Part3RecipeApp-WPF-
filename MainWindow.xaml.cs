using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RecipeApp
{
    public partial class MainWindow : Window
    {
        private List<Recipe> recipes = new List<Recipe>();
        private Recipe currentRecipe;

        public MainWindow()
        {
            InitializeComponent();
            SearchResultsListBox.SelectionChanged += RecipeListBox_SelectionChanged;
        }

        private void AddRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            AddRecipePanel.Visibility = Visibility.Visible;
            DisplayPanel.Visibility = Visibility.Collapsed;
            SearchPanel.Visibility = Visibility.Collapsed;
            currentRecipe = new Recipe();
        }

        private void GenerateIngredientFields_Click(object sender, RoutedEventArgs e)
        {
            int ingredientCount = int.Parse(IngredientsCountTextBox.Text);
            IngredientsPanel.Children.Clear();
            for (int i = 0; i < ingredientCount; i++)
            {
                StackPanel ingredientPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
                TextBox nameTextBox = new TextBox { Width = 150, Margin = new Thickness(5), Tag = "Name" };
                TextBox quantityTextBox = new TextBox { Width = 50, Margin = new Thickness(5), Tag = "Quantity" };
                TextBox unitTextBox = new TextBox { Width = 50, Margin = new Thickness(5), Tag = "Unit" };
                TextBox caloriesTextBox = new TextBox { Width = 50, Margin = new Thickness(5), Tag = "Calories" };
                TextBox foodGroupTextBox = new TextBox { Width = 100, Margin = new Thickness(5), Tag = "FoodGroup" };
                ingredientPanel.Children.Add(new TextBlock { Text = "Name:", Width = 50 });
                ingredientPanel.Children.Add(nameTextBox);
                ingredientPanel.Children.Add(new TextBlock { Text = "Quantity:", Width = 60 });
                ingredientPanel.Children.Add(quantityTextBox);
                ingredientPanel.Children.Add(new TextBlock { Text = "Unit:", Width = 40 });
                ingredientPanel.Children.Add(unitTextBox);
                ingredientPanel.Children.Add(new TextBlock { Text = "Calories:", Width = 60 });
                ingredientPanel.Children.Add(caloriesTextBox);
                ingredientPanel.Children.Add(new TextBlock { Text = "Food Group:", Width = 80 });
                ingredientPanel.Children.Add(foodGroupTextBox);

                IngredientsPanel.Children.Add(ingredientPanel);
            }
        }

        private void GenerateStepFields_Click(object sender, RoutedEventArgs e)
        {
            int stepsCount = int.Parse(StepsCountTextBox.Text);
            StepsPanel.Children.Clear();
            for (int i = 0; i < stepsCount; i++)
            {
                StackPanel stepPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

                TextBlock stepLabel = new TextBlock { Text = $"Step {i + 1}:", Width = 50 };
                TextBox stepTextBox = new TextBox { Width = 500, Margin = new Thickness(5) };

                stepPanel.Children.Add(stepLabel);
                stepPanel.Children.Add(stepTextBox);

                StepsPanel.Children.Add(stepPanel);
            }
        }

        private void SaveRecipe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure Recipe Name is not empty
                if (string.IsNullOrWhiteSpace(RecipeNameTextBox.Text))
                {
                    MessageBox.Show("Please enter a recipe name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Save Recipe Name
                currentRecipe.Name = RecipeNameTextBox.Text;

                // Save Ingredients
                currentRecipe.Ingredients.Clear(); // Clear existing ingredients to avoid duplicates
                foreach (UIElement element in IngredientsPanel.Children)
                {
                    if (element is StackPanel ingredientPanel)
                    {
                        if (ingredientPanel.Children.Count >= 10) // Ensure there are enough children
                        {
                            string name = ((TextBox)ingredientPanel.Children[1]).Text;
                            if (string.IsNullOrWhiteSpace(name))
                            {
                                MessageBox.Show("Please enter a name for all ingredients.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            double quantity;
                            if (!double.TryParse(((TextBox)ingredientPanel.Children[3]).Text, out quantity))
                            {
                                MessageBox.Show($"Invalid quantity entered for ingredient '{name}'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            string unit = ((TextBox)ingredientPanel.Children[5]).Text;
                            double calories;
                            if (!double.TryParse(((TextBox)ingredientPanel.Children[7]).Text, out calories))
                            {
                                MessageBox.Show($"Invalid calories entered for ingredient '{name}'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            string foodGroup = ((TextBox)ingredientPanel.Children[9]).Text;

                            Ingredient ingredient = new Ingredient
                            {
                                Name = name,
                                Quantity = quantity,
                                Unit = unit,
                                Calories = calories,
                                FoodGroup = foodGroup
                            };

                            currentRecipe.Ingredients.Add(ingredient);
                        }
                        else
                        {
                            MessageBox.Show("Invalid ingredient panel structure encountered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                // Save Steps
                currentRecipe.Steps.Clear(); // Clear existing steps to avoid duplicates
                foreach (UIElement element in StepsPanel.Children)
                {
                    if (element is StackPanel stepPanel)
                    {
                        if (stepPanel.Children.Count >= 2) // Ensure there are enough children
                        {
                            string step = ((TextBox)stepPanel.Children[1]).Text;
                            if (string.IsNullOrWhiteSpace(step))
                            {
                                MessageBox.Show("Please enter a description for all recipe steps.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            currentRecipe.Steps.Add(step);
                        }
                        else
                        {
                            MessageBox.Show("Invalid step panel structure encountered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                // Add currentRecipe to recipes list
                recipes.Add(currentRecipe);

                // Clear input fields after saving
                ClearFields();

                // Update RecipeListBox to display all recipe names in alphabetical order
                UpdateRecipeListBox();

                // Switch visibility to display the saved recipe
                AddRecipePanel.Visibility = Visibility.Collapsed;
                DisplayPanel.Visibility = Visibility.Visible;
                SearchPanel.Visibility = Visibility.Collapsed;

                // Select the saved recipe in RecipeListBox and display its details
                RecipeListBox.SelectedItem = currentRecipe.Name;
                DisplaySelectedRecipeDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the recipe: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Optionally, you can log the exception for further debugging
            }
        }

        private void UpdateRecipeListBox()
        {
            // Sort recipes by name
            recipes = recipes.OrderBy(r => r.Name).ToList();

            // Clear and update RecipeListBox
            RecipeListBox.Items.Clear();
            foreach (var recipe in recipes)
            {
                RecipeListBox.Items.Add(recipe.Name);
            }
        }

        private void DisplaySelectedRecipeDetails()
        {
            if (RecipeListBox.SelectedItem != null)
            {
                string selectedRecipeName = RecipeListBox.SelectedItem.ToString();
                Recipe selectedRecipe = recipes.FirstOrDefault(r => r.Name == selectedRecipeName);
                if (selectedRecipe != null)
                {
                    // Display selected recipe details
                    RecipeOutputTextBlock.Text = $"Recipe: {selectedRecipe.Name}\n\n";

                    // Display ingredients
                    RecipeOutputTextBlock.Text += "Ingredients:\n";
                    foreach (var ingredient in selectedRecipe.Ingredients)
                    {
                        RecipeOutputTextBlock.Text += $"{ingredient}\n";
                    }
                    RecipeOutputTextBlock.Text += "\n";

                    // Display steps
                    RecipeOutputTextBlock.Text += "Steps:\n";
                    for (int i = 0; i < selectedRecipe.Steps.Count; i++)
                    {
                        RecipeOutputTextBlock.Text += $"{i + 1}. {selectedRecipe.Steps[i]}\n";
                    }

                    // Calculate total calories
                    double totalCalories = selectedRecipe.CalculateTotalCalories();

                    // Display total calories
                    TotalCaloriesTextBlock.Text = $"Total Calories: {totalCalories}";

                    // Check if total calories exceed 300
                    if (totalCalories > 300)
                    {
                        MessageBox.Show("Warning: This recipe exceeds 300 calories!", "Calorie Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            else
            {
                // Clear text blocks if no recipe is selected
                RecipeOutputTextBlock.Text = "";
                TotalCaloriesTextBlock.Text = "";
            }
        }



        private void DisplayRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            AddRecipePanel.Visibility = Visibility.Collapsed;
            DisplayPanel.Visibility = Visibility.Visible;
            SearchPanel.Visibility = Visibility.Collapsed;

            UpdateRecipeListBox(); // Update RecipeListBox to display recipes in alphabetical order
        }

        private void RecipeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsListBox.SelectedItem != null)
            {
                Recipe selectedRecipe = SearchResultsListBox.SelectedItem as Recipe;
                if (selectedRecipe != null)
                {
                    // Display selected recipe details
                    RecipeOutputTextBlock.Text = selectedRecipe.ToString();
                    TotalCaloriesTextBlock.Text = $"Total Calories: {selectedRecipe.CalculateTotalCalories()}";
                }
            }
        }


        private void SearchRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            AddRecipePanel.Visibility = Visibility.Collapsed;
            DisplayPanel.Visibility = Visibility.Collapsed;
            SearchPanel.Visibility = Visibility.Visible;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchTextBox.Text.ToLower();
            SearchResultsListBox.Items.Clear();

            foreach (var recipe in recipes)
            {
                // Check if the recipe name contains the query
                if (recipe.Name.ToLower().Contains(query))
                {
                    SearchResultsListBox.Items.Add(recipe); // Add entire recipe object
                    continue; // Move to the next recipe if found in name
                }

                // Check if any ingredient name or food group contains the query
                foreach (var ingredient in recipe.Ingredients)
                {
                    if (ingredient.Name.ToLower().Contains(query) || ingredient.FoodGroup.ToLower().Contains(query))
                    {
                        SearchResultsListBox.Items.Add(recipe); // Add entire recipe object
                        break; // Move to the next recipe if found in ingredients
                    }
                }

                // Check if total calories contain the query
                if (recipe.CalculateTotalCalories().ToString().Contains(query))
                {
                    SearchResultsListBox.Items.Add(recipe); // Add entire recipe object
                }
            }
        }


        private void ScaleRecipe_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && SearchResultsListBox.SelectedItem != null)
            {
                double scale = double.Parse(button.Tag.ToString());
                Recipe selectedRecipe = SearchResultsListBox.SelectedItem as Recipe;

                // Scale the recipe
                selectedRecipe.Scale(scale);

                // Update the UI to reflect the scaled quantities
                UpdateIngredientsPanel();

                // Update displayed total calories after scaling
                TotalCaloriesTextBlock.Text = $"Total Calories: {selectedRecipe.CalculateTotalCalories()}";
            }
        }


        private void UpdateIngredientsPanel()
        {

            IngredientsPanel.Children.Clear();
            Recipe selectedRecipe = SearchResultsListBox.SelectedItem as Recipe;

            foreach (var ingredient in selectedRecipe.Ingredients)
            {
                StackPanel ingredientPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

                TextBox nameTextBox = new TextBox { Text = ingredient.Name, Width = 150, Margin = new Thickness(5), Tag = "Name" };
                TextBox quantityTextBox = new TextBox { Text = ingredient.Quantity.ToString(), Width = 50, Margin = new Thickness(5), Tag = "Quantity" };
                TextBox unitTextBox = new TextBox { Text = ingredient.Unit, Width = 50, Margin = new Thickness(5), Tag = "Unit" };
                TextBox caloriesTextBox = new TextBox { Text = ingredient.Calories.ToString(), Width = 50, Margin = new Thickness(5), Tag = "Calories" };
                TextBox foodGroupTextBox = new TextBox { Text = ingredient.FoodGroup, Width = 100, Margin = new Thickness(5), Tag = "FoodGroup" };

                ingredientPanel.Children.Add(new TextBlock { Text = "Name:", Width = 50 });
                ingredientPanel.Children.Add(nameTextBox);
                ingredientPanel.Children.Add(new TextBlock { Text = "Quantity:", Width = 60 });
                ingredientPanel.Children.Add(quantityTextBox);
                ingredientPanel.Children.Add(new TextBlock { Text = "Unit:", Width = 40 });
                ingredientPanel.Children.Add(unitTextBox);
                ingredientPanel.Children.Add(new TextBlock { Text = "Calories:", Width = 60 });
                ingredientPanel.Children.Add(caloriesTextBox);
                ingredientPanel.Children.Add(new TextBlock { Text = "Food Group:", Width = 80 });
                ingredientPanel.Children.Add(foodGroupTextBox);

                IngredientsPanel.Children.Add(ingredientPanel);
            }
        }

        private void ResetQuantities_Click(object sender, RoutedEventArgs e)
        {
            // Reset quantities logic here
        }


        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            recipes.Clear(); // Clear the recipes list

            RecipeListBox.Items.Clear(); // Clear RecipeListBox items

            RecipeOutputTextBlock.Text = ""; // Clear text displayed in RecipeOutputTextBlock

            TotalCaloriesTextBlock.Text = ""; // Clear text displayed in TotalCaloriesTextBlock

            ClearFields(); // Call ClearFields method to clear all input fields
        }

        private void ClearFields()
        {
            RecipeNameTextBox.Clear();
            IngredientsCountTextBox.Clear();
            StepsCountTextBox.Clear();
            IngredientsPanel.Children.Clear();
            StepsPanel.Children.Clear();
        }
        private void ExitApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

    // Ingredient class with food group

    public class Ingredient
    {
        public string Name { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
        public double Calories { get; set; }
        public string FoodGroup { get; set; } // Added FoodGroup property

        public override string ToString()
        {
            return $"{Quantity} {Unit} of {Name} ({FoodGroup}) - {Calories} calories";
        }
    }

    // Recipe class remains the same

    public class Recipe
    {
        private List<Ingredient> originalIngredients; // Store original ingredients for resetting
        public string Name { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        public List<string> Steps { get; set; }

        public Recipe()
        {
            Ingredients = new List<Ingredient>();
            Steps = new List<string>();
            originalIngredients = new List<Ingredient>();
        }
        public void SaveOriginalIngredients()
        {
            originalIngredients.Clear();
            foreach (var ingredient in Ingredients)
            {
                originalIngredients.Add(new Ingredient
                {
                    Name = ingredient.Name,
                    Quantity = ingredient.Quantity,
                    Unit = ingredient.Unit,
                    Calories = ingredient.Calories,
                    FoodGroup = ingredient.FoodGroup
                });
            }
        }

        public double CalculateTotalCalories()
        {
            return Ingredients.Sum(i => i.Calories * i.Quantity);
        }

        public void Scale(double factor)
        {
            foreach (var ingredient in Ingredients)
            {
                ingredient.Quantity *= factor;
                ingredient.Calories *= factor; // Update calories accordingly
            }
        }


        public override string ToString()
        {
            var recipeString = new System.Text.StringBuilder();
            recipeString.AppendLine($"Recipe: {Name}");
            recipeString.AppendLine("\nIngredients:");
            foreach (var ingredient in Ingredients)
            {
                recipeString.AppendLine(ingredient.ToString());
            }
            recipeString.AppendLine("\nSteps:");
            for (int i = 0; i < Steps.Count; i++)
            {
                recipeString.AppendLine($"Step {i + 1}: {Steps[i]}");
            }
            return recipeString.ToString();
        }
    
    // Method to reset ingredients to original state
    public void ResetIngredients()
    {
        Ingredients.Clear();
        foreach (var originalIngredient in originalIngredients)
        {
            Ingredients.Add(new Ingredient
            {
                Name = originalIngredient.Name,
                Quantity = originalIngredient.Quantity,
                Unit = originalIngredient.Unit,
                Calories = originalIngredient.Calories,
                FoodGroup = originalIngredient.FoodGroup
            });
        }
    }
}
}

