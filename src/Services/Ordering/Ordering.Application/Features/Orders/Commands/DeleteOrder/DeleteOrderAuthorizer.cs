using MediatR.Behaviors.Authorization;
using Ordering.Application.Authorization;

namespace Ordering.Application.Features.Orders.Commands.DeleteOrder
{

    public class DeleteOrderAuthorizer : AbstractRequestAuthorizer<DeleteOrderCommand>
    {




        public override void BuildPolicy(DeleteOrderCommand request)
        {
            UseRequirement(new MustHaveOrderRequirement
            {
                UserName = "swn"
            });
        }
    }
}
