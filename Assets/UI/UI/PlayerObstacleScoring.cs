using UnityEngine;
// Routes the player's existing gesture events to the existing obstacle judge.
public class PlayerObstacleScoring : MonoBehaviour
{
    public InputManager input;
    public ObstacleSpawner spawner;
    public RhythmManager rhythm;
    void OnEnable() { if(input){input.OnPressed+=Jump;input.OnSwipe+=Swipe;} }
    void OnDisable() { if(input){input.OnPressed-=Jump;input.OnSwipe-=Swipe;} }
    void Jump()=>Judge(ObstacleInputType.Jump,0);
    void Swipe(InputManager.Direction direction){
        switch(direction){
            case InputManager.Direction.Up:Judge(ObstacleInputType.LongJump,1);break;
            case InputManager.Direction.Down:Judge(ObstacleInputType.Dodge,0);break;
            case InputManager.Direction.Left:Judge(ObstacleInputType.Brake,0);break;
            case InputManager.Direction.Right:Judge(ObstacleInputType.Accelerate,0);break;
        }
    }
    public void Judge(ObstacleInputType type,float charge){
        if(!rhythm || !rhythm.isPlaying || !spawner || Time.timeScale<=0 || AudioListener.pause)return;
        double now=rhythm.SongTime;
        var obstacle=spawner.FindInputTarget(now);
        if(obstacle && now>=obstacle.TargetTime-obstacle.EarlyWindow && now<=obstacle.TargetTime+obstacle.LateWindow)
            obstacle.EvaluateInput(type,now,charge);
    }
}

