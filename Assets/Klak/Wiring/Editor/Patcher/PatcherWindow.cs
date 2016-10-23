//
// Klak - Utilities for creative coding with Unity
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;

namespace Klak.Wiring.Patcher
{
    // Patcher window class
    public class PatcherWindow : EditorWindow
    {
        #region Class methods

        // Open the patcher window with a given patch.
        public static void OpenPatch(Wiring.Patch patch)
        {
            var window = EditorWindow.GetWindow<PatcherWindow>("Patcher");
            window.Initialize(patch);
            window.Show();
        }

        // Open from the main menu (only open the empty window).
        [MenuItem("Window/Klak/Patcher")]
        static void OpenEmpty()
        {
            OpenPatch(null);
        }

        #endregion

        #region EditorWindow functions

        void OnEnable()
        {
            // Initialize if it hasn't been initialized.
            // (this could be happened when a window layout is loaded)
            if (_graph == null) Initialize(null);

            Undo.undoRedoPerformed += OnUndo;
            EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
            EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
        }

        void OnUndo()
        {
            // Invalidate the graph and force repaint.
            _graph.Invalidate();
            Repaint();
        }

        void OnFocus()
        {
            // Invalidate the graph if the hierarchy was touched while unfocused.
            if (_hierarchyChanged) _graph.Invalidate();
        }

        void OnLostFocus()
        {
            _hierarchyChanged = false;
        }

        void OnHierarchyWindowChanged()
        {
            _hierarchyChanged = true;
        }

        void OnGUI()
        {
            // Do nothing while play mode.
            if (isPlayMode)
            {
                DrawPlaceholderGUI("Patcher is not available in play mode",
                    "You must exit play mode to resume editing.");
                return;
            }

            // Synchronize the graph with the patch at this point.
            _graph.SyncWithPatch();

            // Show the placeholder if the patch is not available.
            if (_graph.patch == null)
            {
                DrawPlaceholderGUI("No patch is selected for editing",
                    "You must select a patch in Hierarchy, then press 'Open Patcher' from Inspector.");
                return;
            }

            _graphGUI.BeginGraphGUI(this, new Rect(0, 0, position.width, position.height));
            _graphGUI.OnGraphGUI();
            _graphGUI.EndGraphGUI();
        }

        #endregion

        #region Private members

        Graph _graph;
        GraphGUI _graphGUI;
        bool _hierarchyChanged;

        // Check if in play mode.
        bool isPlayMode {
            get {
                return EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
            }
        }

        // Initializer (called from OpenPatch)
        void Initialize(Wiring.Patch patch)
        {
            hideFlags = HideFlags.HideAndDontSave;
            _graph = Graph.Create(patch);
            _graphGUI = _graph.GetEditor();
        }

        // Draw the placeholder GUI.
        void DrawPlaceholderGUI(string title, string comment)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(title, EditorStyles.largeLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(comment, EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        #endregion
    }
}
