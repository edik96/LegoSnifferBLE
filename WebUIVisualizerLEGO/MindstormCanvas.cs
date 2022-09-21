using Babylon.Blazor;
using Babylon.Blazor.Chemical;

namespace WebUIVisualizerLEGO
{
    public class MindstormCanvas : BabylonCanvasBase
    {
        protected virtual async Task InitializeSzene(LibraryWrapper LibraryWrapper, string canvasId)
        {
            MyCustomData panelData;
            if (ChemicalData is MyCustomData)
            {
                panelData = (MyCustomData)SceneData;
                MySceneCreator creator = new MySceneCreator(LibraryWrapper, canvasId, panelData);
                await creator.CreateAsync(this);
            }
        }
    }
}
