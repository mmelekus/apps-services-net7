﻿using Microsoft.AspNetCore.SignalR; // Hub
using Northwind.Chat.Models; // UserModel, MessageModel

namespace Northwind.SignalR.Service.Hubs;

public class ChatHub : Hub
{
  // a new instance of ChatHub is created to process each method so
  // we must store user names, connection ids, and groups in a static field
  private static Dictionary<string, UserModel> Users = new();

  public async Task Register(UserModel newUser)
  {
    UserModel user;
    string action = "registered as a new user";

    // try to get a stored user with a match on new user
    if (Users.ContainsKey(newUser.Name))
    {
      user = Users[newUser.Name];

      // remove any existing group registrations
      if (user.Groups is not null)
      {
        foreach (string group in user.Groups.Split(','))
        {
          await Groups.RemoveFromGroupAsync(user.ConnectionId, group);
        }
      }
      user.Groups = newUser.Groups;

      // connection id might have changed if the browser 
      // refreshed so update it
      user.ConnectionId = Context.ConnectionId;

      action = "updated your registered user";
    }
    else
    {
      if (string.IsNullOrEmpty(newUser.Name))
      {
        // assign a GUID for name is they are anonymous
        newUser.Name = Guid.NewGuid().ToString();
      }
      newUser.ConnectionId = Context.ConnectionId;
      Users.Add(key: newUser.Name, value: newUser);
      user = newUser;
    }

    if (user.Groups is not null)
    {
      // a user does not have to belong to any groups
      // but if they do, register them with the Hub

      foreach (string group in user.Groups.Split(','))
      {
        await Groups.AddToGroupAsync(user.ConnectionId, group);
      }
    }

    // send a message to the registering user informing of success

    MessageModel message = new() 
    { 
      From = "SignalR Hub", To = user.Name, 
      Body = string.Format(
        "You have successfully {0} with connection id {1}.",
        arg0: action, arg1: user.ConnectionId)
    };

    IClientProxy proxy = Clients.Client(user.ConnectionId);
    await proxy.SendAsync("ReceiveMessage", message);
  }

  public async Task SendMessage(MessageModel message)
  {
    IClientProxy proxy;

    if (string.IsNullOrEmpty(message.To))
    {
      message.To = "Everyone";
      proxy = Clients.All;
      await proxy.SendAsync("ReceiveMessage", message);
      return;
    }

    // if To has a value, then split it into a list of user and group names
    string[] userAndGroupList = message.To.Split(',');

    foreach (string userOrGroup in userAndGroupList)
    {
      if (Users.ContainsKey(userOrGroup))
      {
        // if the item is in Users then send the message to that user
        // by looking up their connection id in the dictionary
        message.To = $"User: {Users[userOrGroup].Name}";
        proxy = Clients.Client(Users[userOrGroup].ConnectionId);
      }
      else // assume the item is a group name to send the message to
      {
        message.To = $"Group: {userOrGroup}";
        proxy = Clients.Group(userOrGroup);
      }
      await proxy.SendAsync("ReceiveMessage", message);
    }
  }
}
