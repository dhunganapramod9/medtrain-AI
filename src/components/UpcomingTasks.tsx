import React from 'react';

interface Task {
  id: number;
  title: string;
  due: string;
  priority: 'High' | 'Medium' | 'Low';
}

const tasks: Task[] = [
  {
    id: 1,
    title: 'Complete Cardiology Module',
    due: '2024-02-20',
    priority: 'High',
  },
  {
    id: 2,
    title: 'Clinical Skills Assessment',
    due: '2024-02-22',
    priority: 'Medium',
  },
  {
    id: 3,
    title: 'Study Group Meeting',
    due: '2024-02-23',
    priority: 'Low',
  },
];

const UpcomingTasks = () => {
  return (
    <div className="bg-white p-6 rounded-lg shadow">
      <h2 className="text-xl font-semibold mb-4">Upcoming Tasks</h2>
      <div className="space-y-4">
        {tasks.map((task) => (
          <div key={task.id} className="border-l-4 border-primary-500 pl-4 py-2">
            <h3 className="font-semibold">{task.title}</h3>
            <p className="text-sm text-gray-600">Due: {task.due}</p>
            <span className={`text-xs px-2 py-1 rounded ${
              task.priority === 'High' ? 'bg-red-100 text-red-800' :
              task.priority === 'Medium' ? 'bg-yellow-100 text-yellow-800' :
              'bg-green-100 text-green-800'
            }`}>
              {task.priority} Priority
            </span>
          </div>
        ))}
      </div>
    </div>
  );
};

export default UpcomingTasks;