using MediatR.Behaviors.Authorization;
using Ordering.Application.Contracts.Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Authorization
{
    public class MustHaveOrderRequirement : IAuthorizationRequirement
    {
        public string UserName { get; set; }


        class MustHaveOrderRequirementHandler : IAuthorizationHandler<MustHaveOrderRequirement>
        {
            private readonly IOrderRepository _orderRepository;

            public MustHaveOrderRequirementHandler(IOrderRepository orderRepository)
            {
                _orderRepository = orderRepository;
            }

            public async Task<AuthorizationResult> Handle(MustHaveOrderRequirement request, CancellationToken cancellationToken)
            {
                var UserName = request.UserName;
                var order = await _orderRepository.GetOrdersByUserName(UserName);

                if (order.Any())
                    return AuthorizationResult.Succeed();

                return AuthorizationResult.Fail("You don't have a Orders !!"); ;
            }
        }
    }

}
