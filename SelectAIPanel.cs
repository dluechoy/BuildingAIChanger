﻿/* BuildingAIChanger
 * Copyright (c) 2015 Stefan Kaufhold, All rights reserved.
 * 
 * Additional changes µ 2015-06 D Lue Choy
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
 */

using System;
using ColossalFramework.UI;
using UnityEngine;

namespace BuildingAIChanger
{
    internal class SelectAIPanel : UIPanel
    {
        /// <summary>
        /// Suffix used to build assembly qualified names for the stock AI classes
        /// </summary>
        private const String ColossalAssemblyInfo =
            ", Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        private UITextField m_input;
        private UILabel m_label;

        public String value
        {
            get { return m_input.text; }
            set { m_input.text = value; }
        }

        public event PropertyChangedEventHandler<string> eventValueChanged;

        public override void Start()
        {
            base.Start();

            gameObject.AddComponent<EditorController>();

            backgroundSprite = "SubcategoriesPanel";
            color = new Color32(150, 150, 150, 255);
            transform.localPosition = new Vector3(0, .08f, 0);

            width = 393;
            height = 25;

            AddLabel();
            AddInput();

//            PerformLayout();
        }

        private void AddLabel()
        {
            m_label = AddUIComponent<UILabel>();
            m_label.text = "Prefab AI";
            m_label.tooltip = "Prefab AI determines what properties and behaviours a given asset/building/vehicle will have.";
            m_label.textColor = new Color32(125,185,255, 255);
            m_label.disabledTextColor = new Color32(255, 255, 255, 255);
            m_label.width = 181;
            m_label.height = 18;
            m_label.position = new Vector3(0.0f,-4.0f);
        }

        private void AddInput()
        {
            m_input = AddUIComponent<UITextField>();

            m_input.width = 191;
            m_input.height = 22;
            m_input.position = new Vector3(190.0f, 0.0f);

            m_input.builtinKeyNavigation = true;
            m_input.isInteractive = true;
            m_input.readOnly = false;

            m_input.horizontalAlignment = UIHorizontalAlignment.Left;
            m_input.selectionSprite = "EmptySprite";
            m_input.selectionBackgroundColor = new Color32(0, 171, 234, 255);
            m_input.normalBgSprite = "TextFieldPanel";
            m_input.textColor = new Color32(12, 21, 22, 255);
            m_input.disabledTextColor = new Color32(254, 254, 254, 255);
            m_input.padding = new RectOffset(4, 0, 3, 0);
            m_input.color = new Color32(255, 255, 255, 255);
            m_input.disabledColor = new Color32(254, 254, 254, 255);
            m_input.bottomColor = new Color32(255, 255, 255, 255);
            m_input.verticalAlignment = UIVerticalAlignment.Middle;
            m_input.horizontalAlignment = UIHorizontalAlignment.Center;

            m_input.eventTextSubmitted += OnTextChanged;
        }

        private void OnTextChanged(UIComponent component, string value)
        {
            Verify();

            if (eventValueChanged != null)
                eventValueChanged(component, value);
        }

/*
        public override void PerformLayout()
        {
            if (m_label != null && m_input != null)
            {
                m_label.position = new Vector3(0, 0);

                m_input.position = new Vector3(0, 0);
                m_input.transform.Translate(.2f, -.01f, 0);
                m_input.width = width*0.67f;
            }
            base.PerformLayout();
        }
*/

        /// <summary>
        /// Color the text field red or white depending on its validity. As would be consistent with other decoration/asset property text fields.
        /// </summary>
        public void Verify()
        {
            if (IsValueValid())
            {
                m_input.color = Color.white;
            }
            else
            {
                m_input.color = Color.red;
            }
        }

        /// <summary>
        /// Check if the current Value is the Name of an existing AI class
        /// </summary>
        /// <returns></returns>
        public bool IsValueValid()
        {
            var type = TryGetAIType();
            try
            {
                type.Equals(null);
            }
            catch (NullReferenceException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to find the AI type for the current field value. Lookup first searches the Colossal Assembly,
        /// then if no class is found all other loaded assemblies are searched.
        /// </summary>
        /// <returns>Type of the AI class or null if none can be found or the found type is not a subclass of PrefabAI</returns>
        public Type TryGetAIType()
        {
            Type type;
            try
            {
                // Search in colossal assembly first
                try
                {
                    type = Type.GetType(value + ColossalAssemblyInfo, true);

                    return (type.IsSubclassOf(typeof (PrefabAI))) ? type : null;
                }
                catch (TypeLoadException)
                {
                    // If not found, look in all loaded Assemblies
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try
                        {
                            type = a.GetType(value, true);

                            // on success
                            return (type.IsSubclassOf(typeof (PrefabAI))) ? type : null;
                        }
                        catch (TypeLoadException)
                        {
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// Instantiates this class and inserts the UI into the asset editor
        /// </summary>
        public static void Insert()
        {
            var view = UIView.GetAView();
            var uiContainer = view.FindUIComponent("FullScreenContainer");
            var propPanel = uiContainer.Find<UIPanel>("DecorationProperties");
            var propPanelPanel = propPanel.Find<UIScrollablePanel>("Container");
            propPanelPanel.AddUIComponent<SelectAIPanel>();
        }
    }
}