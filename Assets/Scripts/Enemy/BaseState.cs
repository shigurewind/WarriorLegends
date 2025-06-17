

public abstract class BaseState
{
    protected Enemy currentEnemy;
    
    public abstract void OnEnter(Enemy enemy);//enter this state
    public abstract void LogicUpdate();//update in this state
    public abstract void PhysicsUpdate();//fixedupdate in this state
    public abstract void OnExit();//quit state
}
