﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using KesselRun.HomeLibrary.Common.Contracts;
using KesselRun.HomeLibrary.Ui.Core.Config;
using KesselRun.HomeLibrary.UiLogic.Views;

namespace KesselRun.HomeLibrary.Ui.Core
{
    public class Navigator : INavigator
    {
        private readonly Dictionary<Type, Type> _viewTypesCache;
        private readonly Dictionary<string, Stack<Control>> _controlStacks = new Dictionary<string, Stack<Control>>();
        public Control NavigationRootControl { get; set; }

        private Navigator()
        {
            _viewTypesCache = new Dictionary<Type, Type>();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class NavigationServiceSingletonCreator
        {
            static NavigationServiceSingletonCreator()
            {

                
            }

            // Private object instantiated with private constructor
            internal static readonly Navigator UniqueInstance = new Navigator();
        }

        public static Navigator SingleNavigator
        {
            get
            {
                return NavigationServiceSingletonCreator.UniqueInstance;
            }
        }

        public void Navigate(Type toView, Control containerControl)
        {
            containerControl.Controls.Clear();

            var typeOfControl = GetViewTypeFromInterface(toView);
            var constructors = typeOfControl.GetConstructors();
            var destinationView = constructors[0].Invoke(new object[] { });
            var destinationViewControl = (Control)destinationView;

            containerControl.Controls.Add(destinationViewControl);

            ViewScopeManager.ManageScope();
            
            if (destinationViewControl is IHydrateOnFocus)
            {
                ((IHydrateOnFocus)destinationView).HydrateWithDataOnFocus();
            }

            _controlStacks[containerControl.Name].Push(destinationViewControl);
            
        }

        public void NavigateTo(Type view, Control containerControl)
        {
            containerControl.Controls.Clear();

            var typeOfControl = GetViewTypeFromInterface(view);
            var constructors = typeOfControl.GetConstructors();
            var destinationView = constructors[0].Invoke(new object[] { });

            var destinationViewControl = (Control)destinationView;

            containerControl.Controls.Add(destinationViewControl);

            ViewScopeManager.ManageScope();

            if (destinationViewControl is IHydrateOnFocus)
            {
                ((IHydrateOnFocus)destinationView).HydrateWithDataOnFocus();
            }

            ManageStack(destinationViewControl, containerControl);
        }

        private void ManageStack(Control control, Control containerControl)
        {
            if (!_controlStacks.ContainsKey(containerControl.Name))
            {
                _controlStacks.Add(containerControl.Name, new Stack<Control>());
            }
            _controlStacks[containerControl.Name].Push(control);
        }

        private Type GetViewTypeFromInterface(Type type)
        {
            Type viewType;

            if (_viewTypesCache.TryGetValue(type, out viewType))
            {
                return viewType;
            }

            var assembly = Assembly.GetExecutingAssembly();

            foreach (var exportedType in assembly.GetExportedTypes().Where(t => t.IsSubclassOf(typeof(Control))))
            {
                if (type.IsAssignableFrom(exportedType))
                {
                    viewType = exportedType;
                    break;
                }
            }
            
            _viewTypesCache.Add(type, viewType);

            return viewType;
        }


        public void Return(Control containerControl)
        {
            var controlToDestroy = _controlStacks[containerControl.Name].Pop();
            controlToDestroy.Dispose();

            if (_controlStacks[containerControl.Name].Any())
            {
                containerControl.Controls.Clear();
                var control = _controlStacks[containerControl.Name].Peek() as UserControl;

                if (control != null)
                {
                    containerControl.Controls.Add(control);

                    ViewScopeManager.ManageScope();

                    if (control is IHydrateOnFocus)
                    {
                        ((IHydrateOnFocus) control).HydrateWithDataOnFocus();
                    }
                }
            }
        }

        public void ClearNavigationBase(Control containerControl)
        {
            if (_controlStacks.ContainsKey(containerControl.Name))
            {
                var controlToDestroy = _controlStacks[containerControl.Name].Pop();
                ((IClosableView)controlToDestroy).CloseView();
                controlToDestroy.Dispose();
            }
        }

        public void ClearContainer(Control containerControl)
        {
            if (_controlStacks.ContainsKey(containerControl.Name))
            {
                while (_controlStacks[containerControl.Name].Any())
                {
                    var controlToDestroy = _controlStacks[containerControl.Name].Pop();
                    controlToDestroy.Dispose();                    
                }

                containerControl.Controls.Clear();
            }
        }

        public void ClearAll()
        {
            foreach (var controlStack in _controlStacks)
            {
                while (controlStack.Value.Any())
                {
                    var controlToDestroy = controlStack.Value.Pop();
                    controlToDestroy.Dispose();
                }
            }

            _controlStacks.Clear();
        }
    }
}
