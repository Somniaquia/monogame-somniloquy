namespace Somniloquy {
    using System.Collections.Generic;
    
    public class DivisionBox : BoxScreen, IFertile {
        public List<float> DivisionLocations = new();

        public List<Screen> GetChildren() => Children;

        public void InsertElement(int index, BoxScreen screen) {
            Children.Insert(index, screen);
            
            if (Children.Count > 0) {
                if (index == 0) {
                    DivisionLocations.Insert(0, DivisionLocations[0] / 2);
                } else {
                    DivisionLocations.Insert(index, (DivisionLocations[index - 1] + DivisionLocations[index]) / 2);
                }
            }
        }

        public void SetDivision(int index, float value) {
            DivisionLocations[index] = value;
        }
    }
}