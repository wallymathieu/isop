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
            _data = data;
        }
        private readonly IIsopServer _data;
        public override string Name
        {
            get { return "Controller"; }
        }

        protected override bool TryMatch(string constraint, string segment, out string matchedValue)
        {
            try
            {
                _data.GetController(segment);
                matchedValue = segment;
                       return true;
            }
            catch (Exception)
            {
                matchedValue = null;
                return false;            
            }
        }
    }
}
