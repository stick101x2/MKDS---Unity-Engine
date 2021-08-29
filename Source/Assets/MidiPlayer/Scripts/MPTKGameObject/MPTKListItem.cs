using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiPlayerTK
{
    /// <summary>
    /// A list of string with index: midi, preset, bank, drum, ...
    /// </summary>
    public class MPTKListItem
    {
        /// <summary>
        /// Index associated to the label (not to mix up with Position in list): 
        ///! @li @c patch num if patch list, 
        ///! @li @c bank number if bank list, 
        ///! @li @c index in list for midi.
        /// </summary>
        public int Index;

        /// <summary>
        /// Label
        /// </summary>
        public string Label;

        /// <summary>
        /// Position in a list (not to mix up with Index which is a value associated to the Label)
        /// </summary>
        public int Position;
    }

}
