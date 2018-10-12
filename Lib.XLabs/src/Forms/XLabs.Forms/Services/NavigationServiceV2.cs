using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using XLabs.Forms.Mvvm;
using XLabs.Ioc;
using XLabs.Platform.Services;

namespace XLabs.Forms.Services
{
    /// <summary>
    /// Implementations INavigation as a Service that can be registered as a Dependency or in an IoC framework.
    /// </summary>
    /// <seealso cref="INavigationServiceV2" />
    /// <seealso cref="Xamarin.Forms.INavigation" />
    public class NavigationServiceV2 : Xamarin.Forms.INavigation, INavigationServiceV2
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationServiceV2" /> class.
        /// </summary>
        /// <param name="navigation">The navigation.</param>
        /// <exception cref="NullReferenceException">navigation must be valid</exception>
        /// <exception cref="ArgumentNullException"><paramref name="navigation"/> is <see langword="null" />.</exception>
        internal NavigationServiceV2(INavigation navigation)
        {
            if (navigation == null) throw new ArgumentNullException(nameof(navigation));

            NativeNavigation = navigation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService" /> class.
        /// </summary>
        /// <param name="page">The page object that holds a reference to he xamarin navigation service.</param>
        /// <exception cref="NullReferenceException">navigation must be valid</exception>
        internal NavigationServiceV2(Page page) : this(page.Navigation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationServiceV2" /> class.  Uses the default navigation element on the Current.MainPage object
        /// </summary>
        /// <exception cref="NullReferenceException">navigation must be valid</exception>
        internal NavigationServiceV2() : this(Application.Current.MainPage.Navigation)
        {
        }
        #endregion Constructors

        #region Native Properties
        public INavigation NativeNavigation { get; internal set; }
        #endregion Native Properties

        #region Static Methods

        /// <summary>
        /// Initializes the navigation framework.
        /// </summary>
        /// <param name="page">The page that holds the reference to the active Xamarin Forms navigation service.</param>
        /// <param name="container">The dependency container to use for handling registration.</param>
        /// <exception cref="ArgumentNullException"><paramref name="page" /> is <see langword="null" />.</exception>
        /// <exception cref="NullReferenceException">navigation must be valid</exception>
        public static void Init(Page page, IDependencyContainer container = null)
        {
            Init(page?.Navigation, container);
        }

        /// <summary>
        /// Initializes the navigation framework.
        /// </summary>
        /// <param name="navigation">The Xamarin forms navigation service.</param>
        /// <param name="container">The dependency container to use for handling registration.</param>
        /// <exception cref="ArgumentNullException"><paramref name="navigation" /> is <see langword="null" />.</exception>
        /// <exception cref="NullReferenceException">navigation must be valid</exception>
        public static void Init(INavigation navigation, IDependencyContainer container = null)
        {
            if (container == null)
                container = Resolver.Resolve<IDependencyContainer>();

            if (container == null) throw new ArgumentNullException(nameof(container), "IOC Dependency Container must be set");
            if (navigation == null) throw new ArgumentNullException(nameof(navigation), "Navigation cannot be null");

            var nav = new NavigationServiceV2(navigation);

            container.Register<INavigation>(t => nav);
            container.Register<INavigationServiceV2>(t => nav);
        }
        #endregion

        #region Navigation Service Implementation
        /// <summary>
        /// navigate to as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TView">The type of the t view.</typeparam>
        /// <param name="options">The options.</param>
        /// <returns>Task.</returns>
        public async Task NavigateToAsync<TView>(INavigationOptions options = null)
            where TView : class
        {
            var p = Resolver.Resolve<TView>() ?? DependencyService.Get<TView>();

            if (p == null) throw new ArgumentNullException(nameof(TView), "View/Page not registered in IOC framework");

            await NavigateToAsync(p, options).ConfigureAwait(true);
        }

        public async Task NavigateToAsync<TView, TViewModel>(INavigationOptions options = null)
            where TView : class
            where TViewModel : class
        {
            var p = Resolver.Resolve<TView>() ?? DependencyService.Get<TView>();
            if (p == null) throw new ArgumentNullException(nameof(TView), "View/Page not registered in IOC framework");

            var viewModel = Resolver.Resolve<TViewModel>() ?? DependencyService.Get<TViewModel>();

            if (options == null) options = new NavigationOptions();
            options.ViewModel = viewModel;

            await NavigateToAsync(p, options).ConfigureAwait(true);
        }

        public async Task NavigateToAsync<TView, TViewModel>(bool animated)
            where TView : class
            where TViewModel : class
        {
            var p = Resolver.Resolve<TView>() ?? DependencyService.Get<TView>();
            if (p == null) throw new ArgumentNullException(nameof(TView), "View/Page not registered in IOC framework");

            var viewModel = Resolver.Resolve<TViewModel>() ?? DependencyService.Get<TViewModel>();

            await NavigateToAsync(p, new NavigationOptions { Animated = animated, ViewModel = viewModel }).ConfigureAwait(true);
        }

        public async Task NavigateToAsync<TView, TViewModel>(TViewModel viewModel, bool animated)
            where TView : class
            where TViewModel : class
        {
            var p = Resolver.Resolve<TView>();
            if (p == null) throw new ArgumentNullException(nameof(TView), "View/Page not registered in IOC framework");

            await NavigateToAsync(p, new NavigationOptions { Animated = animated, ViewModel = viewModel }).ConfigureAwait(true);
        }

        public async Task NavigateToAsync<TView, TViewModel>(TView view, TViewModel viewModel, bool animated)
            where TView : class
            where TViewModel : class
        {
            await NavigateToAsync(view, new NavigationOptions { Animated = animated, ViewModel = viewModel }).ConfigureAwait(true);
        }

        public async Task NavigateToAsync<TView>(TView view, INavigationOptions options = null)
            where TView : class
        {
            if (view == null) throw new ArgumentNullException(nameof(view), "View/Page must be specified");

            var p = view as Page;

            if (options?.ViewModel != null && (p.BindingContext == null || !p.BindingContext.Equals(options?.ViewModel)))
            {
                p.BindingContext = options.ViewModel;
            }

            switch (options?.NavigationAction)
            {
                case NavigationActionTypes.PopCurrentBefore:
                    {
                        await PopAsync(options.Animated).ConfigureAwait(true);
                    }
                    break;
                case NavigationActionTypes.PopToRootBefore:
                    await PopToRootAsync(options.Animated).ConfigureAwait(true);
                    break;
            }

            if (options?.DisableBackButton == true)
            {
                NavigationPage.SetHasBackButton(p, options.DisableBackButton);
            }

            if (options?.Modal == true)
            {
                await PushModalAsync(p, options?.Animated ?? true);
            }
            else
            {
                await PushAsync(p, options?.Animated ?? true);
            }
        }
        #endregion Navigation Service Implementation

        #region Navigation Aware Event Handlers
        /// <summary>
        /// raise on navigate to as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TView">The type of the view.</typeparam>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <param name="view">The view/page.</param>
        /// <param name="vm">The view model.</param>
        /// <returns>Task.</returns>
        private async Task RaiseOnNavigateToAsync<TView, TViewModel>(TView viewFrom, TView viewTo, TViewModel vm)
            where TView : Page
            where TViewModel : class
        {
            INavigationAware navAware;
            INavigationAwareAsync navAwareAsync;
            Task navTask;

            if (vm != null)
            {
                // Lets trigger the Navigated To implementations on the View Model 
                navAwareAsync = vm as INavigationAwareAsync;
                navTask = navAwareAsync?.OnNavigatingToAsync<TView>();
                if (navTask != null) await Task.WhenAll(navTask).ConfigureAwait(true);

                navAware = vm as INavigationAware;
                navAware?.OnNavigatingTo<TView>(viewTo);
            }

            if (viewTo != null)
            {
                // Lets trigger the Navigated To implementations on the Page 
                // First the async
                navAwareAsync = viewTo as INavigationAwareAsync;
                navTask = navAwareAsync?.OnNavigatingToAsync<TView>(viewFrom);
                if (navTask != null) await Task.WhenAll(navTask).ConfigureAwait(true);

                // Then the non-sync (hopefully no one implemented both :)
                navAware = viewTo as INavigationAware;
                navAware?.OnNavigatingTo<TView>(viewFrom);
            }
        }

        /// <summary>
        /// raise on navigate from as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TView">The type of the view.</typeparam>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <param name="view">The view/page.</param>
        /// <param name="vm">The vm.</param>
        /// <returns>Task.</returns>
        private async Task RaiseOnNavigateFromAsync<TView, TViewModel>(TView viewFrom, TView viewTo, TViewModel vm)
            where TView : Page
            where TViewModel : class
        {
            INavigationAware navAware;
            INavigationAwareAsync navAwareAsync;
            Task navTask;

            if (vm != null)
            {
                // Lets trigger the Navigated To implementations on the View Model 
                navAwareAsync = vm as INavigationAwareAsync;
                navTask = navAwareAsync?.OnNavigatingFromAsync<TView>(viewTo);
                if (navTask != null) await Task.WhenAll(navTask).ConfigureAwait(true);

                navAware = vm as INavigationAware;
                navAware?.OnNavigatingFrom<TView>();
            }

            if (viewTo != null)
            {
                // Lets trigger the Navigated To implementations on the Page 
                // First the async
                navAwareAsync = viewTo as INavigationAwareAsync;
                navTask = navAwareAsync?.OnNavigatingFromAsync<TView>(viewFrom);
                if (navTask != null) await Task.WhenAll(navTask).ConfigureAwait(true);

                // Then the non-sync (hopefully no one implemented both :)
                navAware = viewTo as INavigationAware;
                navAware?.OnNavigatingFrom<TView>(viewFrom);
            }
        }
        #endregion Navigation Aware Event Handlers

        #region XForms INavitation Implementation
        /// <summary>
        /// Removes the specified page from the navigation stack.
        /// </summary>
        /// <param name="page">The page to remove from the navigation stack.</param>
        public void RemovePage(Page page)
        {
            NativeNavigation.RemovePage(page);
        }

        /// <summary>
        /// Inserts a page in the navigation stack before an existing page in the stack.
        /// </summary>
        /// <param name="page">The page to add.</param>
        /// <param name="before">The existing page, before which <paramref name="page" /> will be inserted.</param>
        /// <remarks>To be added.</remarks>
        public void InsertPageBefore(Page page, Page before)
        {
            NativeNavigation.InsertPageBefore(page, before);
        }

        /// <summary>
        /// Asynchronously adds a <see cref="T:Xamarin.Forms.Page" /> to the top of the navigation stack.
        /// </summary>
        /// <param name="page">The <see cref="T:Xamarin.Forms.Page" /> to be pushed on top of the navigation stack.</param>
        /// <returns>A task representing the asynchronous dismiss operation.</returns>
        /// <remarks><para>
        /// The following example shows <see cref="M:Xamarin.Forms.INavigation.Push" /> and <see cref="M:Xamarin.Forms.INavigation.Pop" /> usage:
        /// </para>
        /// <example>
        ///   <code lang="C#"><![CDATA[
        /// var newPage = new ContentPage ();
        /// await Navigation.PushAsync (newPage);
        /// Debug.WriteLine ("the new page is now showing");
        /// var poppedPage = await Navigation.PopAsync ();
        /// Debug.WriteLine ("the new page is dismissed");
        /// Debug.WriteLine (Object.ReferenceEquals (newPage, poppedPage)); //prints "true"
        /// ]]></code>
        /// </example></remarks>
        public async Task PushAsync(Page page)
        {
            await RaiseOnNavigateToAsync(NavigationStack.First(), page, page?.BindingContext);

            await NativeNavigation.PushAsync(page);
        }

        /// <summary>
        /// Asynchronously removes the most recent <see cref="T:Xamarin.Forms.Page" /> from the navigation stack.
        /// </summary>
        /// <returns>The <see cref="T:Xamarin.Forms.Page" /> that had been at the top of the navigation stack.</returns>
        /// <remarks>To be added.</remarks>
        public async Task<Page> PopAsync()
        {
            var page = await NativeNavigation.PopAsync();

            await RaiseOnNavigateFromAsync(NavigationStack.First(), page, page.BindingContext);

            return page;
        }

        /// <summary>
        /// Pops all but the root <see cref="T:Xamarin.Forms.Page" /> off the navigation stack.
        /// </summary>
        /// <returns>A task representing the asynchronous dismiss operation.</returns>
        /// <remarks>To be added.</remarks>
        /// TODO Edit XML Comment Template for PopToRootAsync
        public Task PopToRootAsync()
        {
            return NativeNavigation.PopToRootAsync();
        }

        /// <summary>
        /// Presents a <see cref="T:Xamarin.Forms.Page" /> modally.
        /// </summary>
        /// <param name="page">The <see cref="T:Xamarin.Forms.Page" /> to present modally.</param>
        /// <returns>An awaitable Task, indicating the PushModal completion.</returns>
        /// <remarks><para>The following example shows PushModalAsync and PopModalAsync usage:</para>
        /// <example>
        ///   <code lang="C#"><![CDATA[
        /// var modalPage = new ContentPage ();
        /// await Navigation.PushModalAsync (modalPage);
        /// Debug.WriteLine ("The modal page is now on screen");
        /// var poppedPage = await Navigation.PopModalAsync ();
        /// Debug.WriteLine ("The modal page is dismissed");
        /// Debug.WriteLine (Object.ReferenceEquals (modalPage, poppedPage)); //prints "true"
        /// ]]></code>
        /// </example></remarks>
        public async Task PushModalAsync(Page page)
        {
            await RaiseOnNavigateToAsync(NavigationStack.First(), page, page?.BindingContext);

            await NativeNavigation.PushModalAsync(page);
        }

        /// <summary>
        /// Asynchronously dismisses the most recent modally presented <see cref="T:Xamarin.Forms.Page" />.
        /// </summary>
        /// <returns>An awaitable Task&lt;Page&gt;, indicating the PopModalAsync completion. The Task.Result is the Page that has been popped.</returns>
        /// <remarks><para>The following example shows PushModal and PopModal usage:</para>
        /// <example>
        ///   <code lang="C#"><![CDATA[
        /// var modalPage = new ContentPage ();
        /// await Navigation.PushModalAsync (modalPage);
        /// Debug.WriteLine ("The modal page is now on screen");
        /// var poppedPage = await Navigation.PopModalAsync ();
        /// Debug.WriteLine ("The modal page is dismissed");
        /// Debug.WriteLine (Object.ReferenceEquals (modalPage, poppedPage)); //prints "true"
        /// ]]></code>
        /// </example>
        /// <block subset="none" type="note">
        ///   <para>Application developers must <see langword="await" /> the result of <see cref="M:Xamarin.Forms.INavigation.PushModal" /> and <see cref="M:Xamarin.Forms.INavigation.PopModal" />. Calling <see cref="M:System.Threading.Tasks.Task.Wait" /> may cause a deadlock if a previous call to <see cref="M:Xamarin.Forms.INavigation.PushModal" /> or <see cref="M:Xamarin.Forms.INavigation.PopModal" /> has not completed.</para>
        /// </block></remarks>
        public async Task<Page> PopModalAsync()
        {
            var page = await NativeNavigation.PopModalAsync();

            await RaiseOnNavigateFromAsync(NavigationStack.First(), page, page?.BindingContext);

            return page;
        }

        /// <summary>
        /// Asynchronously adds a <see cref="T:Xamarin.Forms.Page" /> to the top of the navigation stack, with optional animation.
        /// </summary>
        /// <param name="page">The page to navigate to.</param>
        /// <param name="animated">Idicanted if the navigation should be animated.</param>
        public async Task PushAsync(Page page, bool animated)
        {
            await RaiseOnNavigateToAsync(NavigationStack.First(), page, page?.BindingContext);

            await NativeNavigation.PushAsync(page, animated);
        }

        /// <summary>
        /// Asynchronously removes the most recent <see cref="T:Xamarin.Forms.Page" /> from the navigation stack, with optional animation.
        /// </summary>
        /// <param name="animated">Idicanted if the navigation should be animated.</param>
        /// <returns>A page object</returns>
        public async Task<Page> PopAsync(bool animated)
        {
            var page = await NativeNavigation.PopAsync(animated);

            await RaiseOnNavigateFromAsync(NavigationStack.First(), page, page?.BindingContext);

            return page;
        }

        /// <summary>
        /// Pops all but the root <see cref="T:Xamarin.Forms.Page" /> off the navigation stack, with optional animation.
        /// </summary>
        /// <param name="animated">Idicanted if the navigation should be animated.</param>
        /// <returns>A page object</returns>
        public Task PopToRootAsync(bool animated)
        {
            return NativeNavigation.PopToRootAsync(animated);
        }

        /// <summary>
        /// Presents a <see cref="T:Xamarin.Forms.Page" /> modally, with optional animation.
        /// </summary>
        /// <param name="page">The page to navigate to.</param>
        /// <param name="animated">Idicanted if the navigation should be animated.</param>
        public async Task PushModalAsync(Page page, bool animated)
        {
            await RaiseOnNavigateToAsync(NavigationStack.First(), page, page?.BindingContext);

            await NativeNavigation.PushModalAsync(page, animated);
        }

        /// <summary>
        /// Asynchronously dismisses the most recent modally presented <see cref="T:Xamarin.Forms.Page" />, with optional animation.
        /// </summary>
        /// <param name="animated">Idicanted if the navigation should be animated.</param>
        /// <returns>A page object</returns>
        public async Task<Page> PopModalAsync(bool animated)
        {
            var page = await NativeNavigation.PopModalAsync(animated);

            await RaiseOnNavigateFromAsync(NavigationStack.First(), page, page?.BindingContext);

            return page;
        }

        /// <summary>
        /// Gets the stack of pages in the navigation.
        /// </summary>
        public IReadOnlyList<Page> NavigationStack { get { return NativeNavigation.NavigationStack; } }
        /// <summary>
        /// Gets the modal navigation stack.
        /// </summary>
        public IReadOnlyList<Page> ModalStack { get { return NativeNavigation.ModalStack; } }
        #endregion XForms INavitation Implementation

    }
}