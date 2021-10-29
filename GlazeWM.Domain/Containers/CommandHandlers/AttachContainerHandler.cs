using GlazeWM.Domain.Containers.Commands;
using GlazeWM.Infrastructure.Bussing;

namespace GlazeWM.Domain.Containers.CommandHandlers
{
  class AttachContainerHandler : ICommandHandler<AttachContainerCommand>
  {
    private Bus _bus;
    private ContainerService _containerService;

    public AttachContainerHandler(Bus bus, ContainerService containerService)
    {
      _bus = bus;
      _containerService = containerService;
    }

    public CommandResponse Handle(AttachContainerCommand command)
    {
      var parent = command.Parent;
      var childToAdd = command.ChildToAdd;

      // Get whether the container has focus prior to making any state changes.
      var isFocusedContainer = _containerService.FocusedContainer == childToAdd;

      if (childToAdd.Parent != null)
        _bus.Invoke(new DetachContainerCommand(childToAdd));

      parent.Children.Insert(command.InsertPosition, childToAdd);
      childToAdd.Parent = parent;

      _containerService.ContainersToRedraw.Add(parent);

      // Adjust focus order of ancestors if the attached container is focused.
      if (isFocusedContainer)
        _bus.Invoke(new SetFocusedDescendantCommand(childToAdd));

      return CommandResponse.Ok;
    }
  }
}
