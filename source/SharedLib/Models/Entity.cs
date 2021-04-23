namespace SharedLib.Models
{
    public abstract class Entity
    {
        //Also use these at subclasses for overridden Id attribute
        [Newtonsoft.Json.JsonIgnore] //used by SharedLib
        [System.Text.Json.Serialization.JsonIgnore] //used by MVC Controller
        public virtual string Id { get; set; }
    }
}
