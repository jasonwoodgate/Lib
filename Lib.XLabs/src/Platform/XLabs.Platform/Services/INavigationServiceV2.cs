// ***********************************************************************
// Assembly         : XLabs.Platform
// Author           : XLabs Team
// Created          : 08-14-2016
// 
// Last Modified By : XLabs Team
// Last Modified On : 08-14-2016
// ***********************************************************************
// <copyright file="INavigationServiceV2.cs" company="XLabs Team">
//     Copyright (c) XLabs Team. All rights reserved.
// </copyright>
// <summary>
//       This project is licensed under the Apache 2.0 license
//       https://github.com/XLabs/Xamarin-Forms-Labs/blob/master/LICENSE
//       
//       XLabs is a open source project that aims to provide a powerfull and cross 
//       platform set of controls tailored to work with Xamarin Forms.
// </summary>
// ***********************************************************************
// 
using System.Threading.Tasks;

namespace XLabs.Platform.Services
{
	/// <summary>
	/// Interface INavigationServiceV2
	/// </summary>
	public interface INavigationServiceV2
	{
		/// <summary>
		/// Navigates to View/Page asynchronously.
		/// </summary>
		/// <typeparam name="TView">The type of the t view.</typeparam>
		/// <param name="options">The options.</param>
		/// <returns>Task.</returns>
		Task NavigateToAsync<TView>(INavigationOptions options = null)
			where TView : class;

		/// <summary>
		/// Navigates to View/Page asynchronously and associate the view model with it.
		/// </summary>
		/// <typeparam name="TView">The type of the view.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="animated">if set to <c>true</c> [animated].</param>
		/// <returns>Task.</returns>
		Task NavigateToAsync<TView, TViewModel>(bool animated)
			where TView : class
			where TViewModel : class;

		/// <summary>
		/// Navigates to View/Page asynchronously and associate the view model with it.
		/// </summary>
		/// <typeparam name="TView">The type of the view.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="options">The options.</param>
		/// <returns>Task.</returns>
		Task NavigateToAsync<TView, TViewModel>(INavigationOptions options = null)
			where TView : class
			where TViewModel : class;

		/// <summary>
		/// </summary>
		/// <typeparam name="TView">The type of the view.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="viewModel">The view model.</param>
		/// <param name="animated">if set to <c>true</c> [animated].</param>
		/// <returns>Task.</returns>
		Task NavigateToAsync<TView, TViewModel>(TViewModel viewModel, bool animated)
			where TView : class
			where TViewModel : class;

		/// <summary>
		/// Navigates to View/Page asynchronously and associate the view model with it.
		/// </summary>
		/// <typeparam name="TView">The type of the view.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="view">The view/page.</param>
		/// <param name="viewModel">The view model.</param>
		/// <param name="animated">if set to <c>true</c> [animated].</param>
		/// <returns>Task.</returns>
		Task NavigateToAsync<TView, TViewModel>(TView view, TViewModel viewModel, bool animated)
			where TView : class
			where TViewModel : class;

		/// <summary>
		/// Navigates to View/Page asynchronously and associate the view model with it.
		/// </summary>
		/// <typeparam name="TView">The type of the t view.</typeparam>
		/// <param name="view">The view/page.</param>
		/// <param name="options">The options.</param>
		/// <returns>Task.</returns>
		Task NavigateToAsync<TView>(TView view, INavigationOptions options)
			where TView : class;
	}

	/// <summary>
	/// Interface for Navigation Options
	/// </summary>
	public interface INavigationOptions
	{
		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The view model.</value>
		object ViewModel { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="INavigationOptions" /> is animated.
		/// </summary>
		/// <value><c>true</c> if animated; otherwise, <c>false</c>.</value>
		bool Animated { get; set; }

		/// <summary>
		/// Gets or sets the navigation action.
		/// </summary>
		/// <value>The navigation action.</value>
		NavigationActionTypes NavigationAction { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="INavigationOptions" /> is modal.
		/// </summary>
		/// <value><c>true</c> if modal; otherwise, <c>false</c>.</value>
		bool Modal { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [disable back button].
		/// </summary>
		/// <value><c>true</c> if [disable back button]; otherwise, <c>false</c>.</value>
		bool DisableBackButton { get; set; }
	}

	/// <summary>
	/// Navigation Options.
	/// </summary>
	/// <seealso cref="XLabs.Platform.Services.INavigationOptions" />
	public class NavigationOptions : INavigationOptions
	{
		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The view model.</value>
		public object ViewModel { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether the navigation is animated.
		/// </summary>
		/// <value><c>true</c> if animated; otherwise, <c>false</c>.</value>
		public bool Animated { get; set; } = true;
		/// <summary>
		/// Gets or sets the navigation action type.
		/// </summary>
		/// <value>The navigation action.</value>
		public NavigationActionTypes NavigationAction { get; set; } = NavigationActionTypes.None;
		/// <summary>
		/// Gets or sets a value indicating whether this navigation operation is modal.
		/// </summary>
		/// <value><c>true</c> if modal; otherwise, <c>false</c>.</value>
		public bool Modal { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether the back button should be disabled.
		/// </summary>
		/// <value><c>true</c> if [disable back button]; otherwise, <c>false</c>.</value>
		public bool DisableBackButton { get; set; }
	}

	/// <summary>
	/// Navigation Action Types
	/// </summary>
	public enum NavigationActionTypes
	{
		/// <summary>
		/// No action
		/// </summary>
		None,
		/// <summary>
		/// Pop current view/page before navigation
		/// </summary>
		PopCurrentBefore,
		/// <summary>
		/// Pop to root before navigation
		/// </summary>
		PopToRootBefore
	}
}