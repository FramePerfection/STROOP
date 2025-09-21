namespace STROOP.Models
{
    public interface IUpdatableDataModel
    {
        /// <summary>
        /// Update dependency level 1
        /// </summary>
        void Update();

        /// <summary>
        /// Update dependency level 2
        /// </summary>
        void Update2();
    }
}
