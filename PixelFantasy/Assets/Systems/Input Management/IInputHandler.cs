
namespace Systems.Input_Management
{
    public interface IInputHandler
    {
        void HandleInput();
        void OnEnter();
        void OnExit();
    }
}