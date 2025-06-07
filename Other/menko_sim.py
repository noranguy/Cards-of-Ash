import random
from collections import Counter

class Card:
    def __init__(self, card_type: str, card_class: str):
        self.c_type = card_type
        self.c_class = card_class

    def __str__(self):
        return f'Card(c_type={self.c_type}, c_class={self.c_class})'
    
presets = {
    'default': {
        'starting_hand': [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic')
        ],
        'omamori': 'none'
    }
}

type_to_idx = {'light': 0, 'regular': 1, 'heavy': 2}
num_cards = 6
type_prob = [0.2, 0.7, 0.9]

def draw_card(card, shown=True):
        width = 18
        top = "+" + "-" * (width - 2) + "+"
        empty = "|" + " " * (width - 2) + "|"
        text = f"{card.c_type} {card.c_class}" if shown else "X"
        centered = "|" + text.center(width - 2) + "|"
        return [top, empty, empty, centered, empty, empty, top]

def draw_cards(cards, shown=None):
    if shown is None:
        shown = [True] * len(cards)
        
    card_lines = [draw_card(card, s) for card, s in zip(cards, shown)]

    combined_lines = []
    for i in range(len(card_lines[0])):
        line = "   ".join(card[i] for card in card_lines)
        combined_lines.append(line)

    return "\n".join(combined_lines)

class Agent:
    def __init__(self):
        return
    
    def skill_check(self):
        return 1

    def move(self):
        return -1, None
    
    def backward(self, card: Card):
        return

class Agent0(Agent):
    def __init__(self):
        self.hand = [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic')
        ]
        random.shuffle(self.hand)

        self.omamori = 'none'
        self.table = [False] * num_cards
        self.table_idx = 0
        self.skill = 2
    
    def skill_check(self, threshold=0.5):
        check = random.uniform(0, self.skill)
        return check < threshold

    def move(self):
        self.table_idx = 0
        
        while self.table[self.table_idx]:
            self.table_idx = random.randint(0, 5)

        return self.hand.pop(), (self.table_idx,), (self.skill_check(),)
    
    def backward(self, cards: list[Card]):
        if cards[0] is not None:
            self.table[self.table_idx] = True

class Human(Agent):
    def __init__(self, starting_hand: list[Card], omamori: str):
        self.hand = starting_hand
        self.omamori = omamori

    def skill_check(self):
        return int(input("Skill check result: "))

    def move(self):
        print('Hand: ')
        print(draw_cards(self.hand))
        idx = int(input(f'Which card to use? (number 0-{len(self.hand)-1}) '))
        table_idx = int(input(f'Which card to throw at? (number 0-{num_cards-1}) '))
        return self.hand.pop(idx), (table_idx,), (self.skill_check(),)

    def backward(self, _: tuple[Card]):
        return

class Agent1(Agent):
    def __init__(self):
        self.hand = [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic')
        ]
        self.omamori = 'none'
        self.skill = 1.3

        self.type_freq = [0, 0, 0]

        self.rank = [1, 1, 1]
        mod = [(1-x) * 1/3 for x in type_prob]
        mod_sum = sum(mod)
        h, m, l = (x / mod_sum for x in mod)
        self.rank_mod = [
            [l, m, h],
            [h, l, m],
            [m, h, l]
        ]

        self.idx = 0
        self.order = list(range(num_cards))
        random.shuffle(self.order)

        self.last = None
    
    def skill_check(self, threshold=0.5):
        check = random.uniform(0, self.skill)
        return check < threshold

    def move(self):
        freq = [
            sum(1 for x in self.hand if x.c_type == 'light'),
            sum(1 for x in self.hand if x.c_type == 'regular'),
            sum(1 for x in self.hand if x.c_type == 'heavy')
        ]
        self.hand.sort(key = lambda x: (self.rank[type_to_idx[x.c_type]], freq[type_to_idx[x.c_type]]))
        check = self.skill_check()

        self.last = self.hand[-1].c_type
        return self.hand.pop(), (self.order[self.idx],), (check,)
    
    def backward(self, cards: list[Card]):
        if cards[0] is not None and self.idx < 5:
            self.type_freq[type_to_idx[cards[0].c_type]] += 1
            self.idx += 1
            self.rank = [
                (2 - min(2, self.type_freq[2])) / (6 - self.idx),
                (2 - min(2, self.type_freq[0])) / (6 - self.idx),
                (2 - min(2, self.type_freq[1])) / (6 - self.idx),
            ]
        else:
            self.rank = [a * b for a, b in zip(self.rank, self.rank_mod[type_to_idx[self.last]])]

class Agent2(Agent):
    def __init__(self):
        self.hand = [
            Card(card_type='light', card_class='bouncy'),
            Card(card_type='regular', card_class='bouncy'),
            Card(card_type='heavy', card_class='bouncy'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic')
        ]
        self.omamori = 'none'
        self.skill = 1.2

        self.type_freq = [0, 0, 0]

        self.ranks = [[1/3] * 3] * 6
        self.orders = [list(range(6)) for _ in range(3)]
        mod = [(1-x) * 1/3 for x in type_prob]
        mod_sum = sum(mod)
        h, m, l = (x / mod_sum for x in mod)
        self.rank_mod = [
            [l, m, h],
            [h, l, m],
            [m, h, l]
        ]

        self.last = []
        self.count = 0
    
    def skill_check(self, threshold=0.5):
        check = random.uniform(0, self.skill)
        return check < threshold

    def move(self):
        freq = [
            sum(1 for x in self.hand if x.c_type == 'light'),
            sum(1 for x in self.hand if x.c_type == 'regular'),
            sum(1 for x in self.hand if x.c_type == 'heavy')
        ]

        for idx, row in enumerate(self.orders):
            row.sort(key = lambda x: self.ranks[x][idx], reverse=True)

        self.hand.sort(key = lambda x: (
            (self.ranks[self.orders[w := type_to_idx[x.c_type]][0]][w]
            if x.c_class == 'basic' else
            (self.ranks[self.orders[w := type_to_idx[x.c_type]][0]][w] + self.ranks[self.orders[w][1]][w]),
            freq[type_to_idx[x.c_type]])
        ))

        if self.hand[-1].c_class == 'bouncy':
            check1 = self.skill_check()
            check2 = self.skill_check()
            order1 = self.orders[type_to_idx[self.hand[-1].c_type]][0]
            order2 = self.orders[type_to_idx[self.hand[-1].c_type]][1]
            self.last = [self.hand[-1].c_type, order1, order2]
            return self.hand.pop(), (order1, order2), (check1, check2)
        else:
            check = self.skill_check()
            order = self.orders[type_to_idx[self.hand[-1].c_type]][0]
            self.last = [self.hand[-1].c_type, order]
            return self.hand.pop(), (order,), (check,)
    
    def backward(self, cards: list[Card]):
        if self.count == 6: return

        old_prob = [
            (2 - min(2, self.type_freq[2])) / (6 - self.count),
            (2 - min(2, self.type_freq[0])) / (6 - self.count),
            (2 - min(2, self.type_freq[1])) / (6 - self.count),
        ]
        for card in cards:
            if card is not None:
                self.type_freq[type_to_idx[card.c_type]] += 1
                self.count += 1
        
        if self.count == 6: return
        
        new_prob = [
            (2 - min(2, self.type_freq[2])) / (6 - self.count),
            (2 - min(2, self.type_freq[0])) / (6 - self.count),
            (2 - min(2, self.type_freq[1])) / (6 - self.count),
        ]

        self.ranks = [[(row[x] / old_prob[x] * new_prob[x] if old_prob[x] != 0 else 0) for x in range(3)] for row in self.ranks]

        for card, idx in zip(cards, self.last[1:]):
            if card is not None:
                self.ranks[idx] = [-1e5, -1e5, -1e5]
            else:
                self.ranks[idx] = [a * b for a, b in zip(self.ranks[idx], self.rank_mod[type_to_idx[self.last[0]]])]

class Game:
    def __init__(self, agent1: Agent, agent2: Agent, verbose=True):
        self.agent1 = agent1
        self.table1 = [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic')
        ]
        random.shuffle(self.table1)
        self.shown1 = [False] * num_cards

        self.agent2 = agent2
        self.table2 = [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic')
        ]
        random.shuffle(self.table2)
        self.shown2 = [False] * num_cards

        self.flip_chance = [
            [1, 0, 2],
            [2, 1, 0],
            [0, 2, 1]
        ]
        self.verbose = verbose

        self.count1 = 0
        self.count2 = 0

        if self.verbose:
            print('Agent 1:')
            print(draw_cards(self.table1, self.shown1))
            print('Agent 2:')
            print(draw_cards(self.table2, self.shown2))

    def update1(self, idx):
        self.shown1[idx] = True
        self.count1 += 1
    
    def update2(self, idx):
        self.shown2[idx] = True
        self.count2 += 1

    def turn(self, agent: Agent, table: list[Card], update, name):
        card, table_idx, check = agent.move()
        
        result = []
        for i, c in zip(table_idx, check):
            threshold = type_prob[self.flip_chance[type_to_idx[card.c_type]][type_to_idx[table[i].c_type]]]
            if random.random() < threshold * c:
                if self.verbose:
                    print(f'Agent {name} threw {str(card)} at {i} and it flipped')
                update(i)
                result.append(table[i])
            else:
                if self.verbose:
                    print(f'Agent {name} threw {str(card)} at {i} and it did not flip')
                result.append(None)

        agent.backward(result)

    def round(self):
        self.turn(self.agent1, self.table1, self.update1, 1)
        self.turn(self.agent2, self.table2, self.update2, 2)
        
        if self.verbose:
            print('Agent 1:')
            print(draw_cards(self.table1, self.shown1))
            print('Agent 2:')
            print(draw_cards(self.table2, self.shown2))

        return sum(self.shown1) == num_cards or sum(self.shown2) == num_cards

    def winner(self):
        if self.count1 > self.count2:
            return 1
        elif self.count1 < self.count2:
            return 2
        else:
            play1, play2 = 0, 0
            threshold = 0.5
            while play1 == play2:
                play1 = self.agent1.skill_check(threshold)
                play2 = self.agent2.skill_check(threshold)
                if threshold > 0.05:
                    threshold * 0.9

            return int(play1 < play2) + 1

results = []
verbose = False

for i in range(10000):
    human = Human(**presets['default'])
    game = Game(Agent1(), Agent2(), verbose=verbose)

    for i in range(6):
        if verbose:
            print(f'\n\nRound {i+1}!!')
        if game.round(): break

    results.append(game.winner())

print(Counter(results))