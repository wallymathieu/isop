using System;
using Nancy;
using Nancy.Hosting.Self;
using Nancy.Routing.Constraints;

namespace Isop.Server
{
    public class ControllerSegmentConstraint : RouteSegmentConstraintBase<string>
    {
        public ControllerSegmentConstraint(IIsopServer data)
        {
            this.data = data;
        }
        IIsopServer data;
        public override string Name
        {
            get { return "Controller"; }
        }

        protected override bool TryMatch(string constraint, string segment, out string matchedValue)
        {
            try
            {
                data.GetController(segment);
                matchedValue = segment;
                       return true;
            }
            catch (Exception ex)
            {
                matchedValue = null;
                return false;            
            }
        }
    }
}
