using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SharpMarker
{
    interface ICanvasTool
    {
        void OnMouseDown(IInputElement relativeTo, MouseButtonEventArgs e);

        void OnMouseUp(IInputElement relativeTo, MouseButtonEventArgs e);

        void OnMouseMove(IInputElement relativeTo, MouseEventArgs e);

        event EventHandler Completed;

        event EventHandler OverlayUpdated;
    }
}
